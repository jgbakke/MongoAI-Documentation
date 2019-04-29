# MongoAI-Documentation
Documentation for MongoAI Project

As a Unity Developer, I always strive to create useful tools for myself and other developers. One issue that I have been having with a game I've been working on is developing challenging and engaging AI for players. This project solves that issue.

Genetic Algorithms are a good solution to optimize and evolve parameters in game development. However, they require a very large dataset to be effective. MongoAI solves that issue by providing a Backend-as-a-Service solution that aggregates all AI data on every single instance of your game. A user (a game developer) can program their game to "save" the data. Under the hood, this tool will send their data to Stitch, which will store it in a MongoDB Atlas instance. When you want to Instantiate a new object with optimized AI parameters, you just tell the tool you want a new child, and MongoAI will look through the Atlas Database, pick the fittest "genes", and apply a Genetic Algorithm which gives you back your parameters.

Tests I conducted have showed how well this data aggregation feature works. I first tried multiple tests with an Example project to see how long evolution would take. In this example, there is a Box with RGB values, and these RGB values would evolve over time to achieve a desired color.

When running 1 instance, it took on average 53 generations--a long time for evolution for a game. When running 2 instances concurrently, it took about 16 generations per instance. When running 4 instances, it took less than 10 generations per instance.

By collecting more data from every single instance, MongoAI enables faster evolution for your parameters. In a world where gamers are smart and always getting better, you need to quickly evolve, or your AI will no longer remain exciting and engaging to play against. The speed at which MongoAI enables evolution will present a challenge to your players and keep them involved with your game.

Currently there is a Demo Mac App that you can download. The Demo app will soon be ported to Windows as well. The MongoAI Driver currently supports both Mac and Windows.

## Getting Started
This is currently in Beta. If you would like to be a part of the Beta, please contact me.

Download the MongoAI Driver Unity Package. (Only available to those part of Beta)
Import the Package in Unity
Go to https://www.youtube.com/watch?v=FJ308O_f0IM at the 4:00 minute mark if you do not know how to import packages to Unity. It is a simple process.

Change your .NET Scripting Runtime to .NET 4.x Equivalent. Restart your editor and change API Compatability Level to 4.0.

Follow along with Box.cs

## Setting up Your Class
Define public variables at the top of your code. All genetic properties should be floats.

```
    public float red = 0.2f;
    public float green = 0.2f;
    public float blue = 0.2f;
```

Now you also need to specify which variables you want to be considered genes. By default none will be. Add this code

```
public List<string> geneticProperties = new List<string>();
```

And modify in the Unity Editor to add the properties you want to be considered genes. Spell them exactly as they are in the code, *not* how they appear in the Editor. The Editor automatically reformats spacing and capitalization, so you must name them how they appear in the code.

## Quick Evolution vs Caching
You should decide if rapid evolution or caching is more important to you, and then follow the corresponding tutorial.

Quick Evolution helps your AI evolve quicker. However, it is *slow*. Running a genetic algorithm and connecting to a cloud database is expensive. This is what the Demo app used because it is not concerned with performance. However, if you will be concerned with performance, this is not recommended.

Caching does not evolve as fast because you load one giant batch of children and keep them cached. Therefore, as the game goes on, your cache does not evolve, because they are not being reloaded. This is fast because all you do is request a new child from the cache.

## Quick Evolution

Whenever you want to update your genetic properties, call this function.

```
MongoAI.manager.PopulateProperties(this, geneticProperties, <name of this class>, <Number of Chromosomes to fetch>, <mutation percent>, <crossover?>);
```

Number of chromosomes is how many will be retrieved. If you pick 5 for example, the 5 most fit individuals will be used in the Genetic Algorithm. 1 of these 5 at random will be chosen, and another 1 will be crossed over with it. Therefore, your new chromosome could be made up of the 4th and 5th most fit individuals.

Mutation percent: On a scale of 0 to 1, each gene may vary by up to this much percent. For example, if you have a property x at 10, and a mutation of 0.1f, mutation can change it at random anywhere from 9.0 to 11.0.

Crossover: True to enable genetic crossover, false otherwise. This increases genetic diversity.
