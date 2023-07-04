# ProceduralPopulationDatabase  
A monadic queryable database-like structure for categorized populations.  

<br />
<br />

## Population Census  

One of the major parts of simulating a population is figuring out the distribution of certain sub-groups within that population. For example in a fantasy game you might expect a certain portion of the population to belong to each race within the game world - say, humans, orcs, and elves. You might also want to further sub-divide by factions such as gender, homeland, class/job, and any other number of factors within the game.  

*Procedural Population Database* allows you to easily define such a population census and then sample it to get all or part of any combination of these factors.  

Running with the fantasy-world example from above you could setup a population census where the total population is something like 10,000 people. Within that population you could then specify that exactly half are male and the other half are female.  

```
//define a new population census
var census = new PopulationTree(10_000);

//slice the root into two halves where each sub-range represents differnet sexes
census.Slice(0, new float[] { 0.5f, 0.5f }); 
```

The above creates a tree-like structure where the root is epxressed as an IndexRange object that spans from 0 to 9999. It then creates a second level in the tree where each leaf is a new IndexRange of exactly half of the total population, i.e. 0 to 4999 and 5000 to 9999 respectively. This slice can occur at the next level to provide the different races of our world like so:  

```
//split the depth1 ranges into three more child ranges which represent humans, orcs, and elves
census.Slice(1, new float[] { 0.35f, 0.40f, 0.25f });

```

We now have a third level our tree where each gender is subdivided into the three races of human, orc, and elf for a total of six IndexRanges. Three for male and three for female. Now let's dubdivide further for classes.  

```
census.Slice(2, new float[] { 0.1f, 0.1f, 0.25f, 0.30f, 0.1852f, 0.0648f });
```

Let's also define some enums to make life easier.  

```
enum Depths : int
{
    Root,
    Gender,
    Race,
    Class,
}

enum Sexes
{
    Male,
    Female,
}

enum Races
{
    Human,
    Orc,
    Elf,
}

enum Classes
{
    Rogue,
    Mages,
    Paladin,
    Fighter,
    Assassin,
    Necromancer,
}
```


Sounds great and all but what can we do with this? Well, read on!  

<br />
<br />

## Queryable  

Similar to other database or Linq expressions it is possible to query the system in a monadic fashion. Let's now get the total index range of our population that represents Orc Paladins, which can be either male or female.  

```
PopulationSample paladinOrcPop = census.Query()
                .Query((int)Depths.Class, (int)Classes.Paladin)
                .Query((int)Depths.Race, (int)Races.Orc);
```

With this statement we now have access to exactly the indices within the total 10,000 population that correspond to the male orc paladins and female orc paladins and nothing else. If we wanted we could drop, say, the race query and we'd get the list of all paladins for all races and sexes. Or we could specify the sex and narrow it down to a single IndexRange.

### IndexRange
So what even is an IndexRange? Well it's defined simply as this:

```
readonly public struct IndexRange
{
    readonly public int StartIndex;
    readonly public int Length;
    public int EndIndex => StartIndex + Length - 1;
    public bool Contains(int index) => Length > 0 && index >= StartIndex && index <= EndIndex;
}
```   

And all it does is specify a range of numbers within our initial population that correspond to the queried portion of that population. We could easily get the entire population simply with:  

```
PopulationSample totalPop = census.Query();

//get all indexes within the total population
IndexRange range = totalPop.Ranges[0];
```

Likewise we could get the indicies of our previous orc paladin population with:  

```
IndexRange maleOrcPaladins = paladinOrcPop.Ranges[0];
IndexRange femaleOrcPaladins = paladinOrcPop.Ranges[1];
```

By using IndexRanges rather than List<int> we can simply compact the data into runs of contiugous numbers and save a lot of space this way. At the same time we can still get a unique index for every person within the specified sub-division of a population.  

<br />
<br />

## Procedural

But some of you might be wondering, "Cool. I can get a list of numbers. But so what? They aren't the actual people in the world." Indeed. BUT each of these values within an index range effectively represents a unique identifier for each person of the population. And due to the tree-like nature of the census itself you can quickly take an given indentifier and re-map it back to the portion of the population in which it belongs.  

For example, let's say I have my previous population of orc paladins. I can do something like this to get an unique id for one of them.

```
int uid = orcPaladins.RandomId();
```

Let's say this uid ends up with a value of 7151. I can map this to a unique individual in my game that is created as a female orc paladin. If I later need to save the game or perhaps unload and area with this character in it I can destroy them and simply store this 32-bit unique id. Later, I can use this unique id to re-create the exact same character. But how will I know from this simple number what gender, race, and class they were? Well as I said before this unquie id maps within the total population which was sub-divided in a tree where each sun-division is a seperate level. So we can very quickly determine exactly in what portion of the census this id lies using this:  

```
List<int> popCategories = census.MapId(uid); //uid is the value 7151 here
```

The returned list of values will correspond to the enumerated indicies of our categories defined above. So in this case it would be something like  

```
popCategories = [1, 1, 2]
```

The first value of 1 will be our sex which maps to *Sexes.Female*, the second value of 1 is our race which maps to *Races.Orc*, and the third value of 2 is our class which maps to *Classes.Paladin*.  

And with that we are easily able to query up random portions of our total population, store them away very efficiently when required, and then re-create our original queried dataset at a later time!  

