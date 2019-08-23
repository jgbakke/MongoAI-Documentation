# Demo Videos
[Platformer Demo](https://www.youtube.com/watch?v=zhdwVdtIMCU)
[Box Demo](https://www.youtube.com/watch?v=EOk0At5IzqI)

# MongoAI-Documentation
Documentation for MongoAI Project

MongoAI is a Backend-as-a-Service tool to automatically evolve your AI by collecting data from every instance of your game running worldwide and applying an evolutionary algorithm. MongoAI accelerates your AI development by enabling you to run many multiple instances in development before release to find your best parameters, or to evolve in response to players.

By collecting more data from every single instance, MongoAI enables faster evolution for your parameters. In a world where gamers are smart and always getting better, you need to quickly evolve, or your AI will no longer remain exciting and engaging to play against. The speed at which MongoAI enables evolution will present a challenge to your players and keep them involved with your game.

Download the 2 Demo Apps from the demos directory and watch how it works. You can also try running multiple instances on your machine or another machine at once to observe how they will evolve quicker. The MongoAI-related source code for these is available in the examples directory, so you can follow along with the code.

The MongoAI Driver currently supports both Mac OS X and Windows.

# Tutorial

## Getting Started
This is currently in Beta. If you would like to be a part of the Beta, please contact me.

Download the MongoAI Unity Package.
**Note that this package is currently only available to those in Beta. It will be made publicly available in the future. Contact me if you would like to be part of the beta.**

Import the Package in Unity.

Change your .NET Scripting Runtime to .NET 4.x Equivalent. Restart your editor and change API Compatability Level to 4.0.

Go to Build Settings->Player Settings. Change your Company Name and the Game Name. These attributes are used by MongoAI to identify your game and prevent storage collisions with other games. If you do not change your game or company, you **will** retrieve data from another company's example projects when you try to run it, or any game and company that uses an equivalent identifier.

![picture](https://raw.githubusercontent.com/jgbakke/MongoAI-Documentation/master/Company%20Name%20Change.png)

**You should always change your Company Name and Game Name at the start of a new project. You may get collisions with other games if you do not.**

Follow along with Box.cs. Box.cs is the code for the Box Demo Game. In this example, we use a genetic algorithm to evolve a box's color to a color specified in the editor.

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
MongoAI.manager.PopulateProperties(this, geneticProperties, <name of this class>, <Number of Most Fit Individuals to fetch>, <mutation percent>, <crossover?>);
```

Number of chromosomes is how many will be retrieved. If you pick 5 for example, the 5 most fit individuals will be used in the Genetic Algorithm. 1 of these 5 at random will be chosen, and another 1 will be crossed over with it. Therefore, your new chromosome could be made up of the 4th and 5th most fit individuals.

Mutation percent: On a scale of 0 to 1, each gene may vary by up to this much percent. For example, if you have a property x at 10, and a mutation of 0.1f, mutation can change it at random anywhere from 9.0 to 11.0.

Crossover: True to enable genetic crossover, false otherwise. This increases genetic diversity.

## Caching

If you want to cache properties instead, call this.

```
MongoAI.manager.CacheData(className, <number to cache>);
```

This generates however many offspring you want in the cache. Because MongoAI.manager is accessible to all classes, anybody can use the cache now. When you want to get updated genetic properties, just call

```
MongoAI.manager.PopulateFromCache(this, geneticProperties, <name of this class>, <Number of Most Fit Individuals to fetch>, <mutation percent>, <crossover?>);
```

This works exactly the same as PopulateProperties except the data is already cached so it is very fast.

## Saving Data
When you want to save data call:

```
MongoAI.manager.SaveData(this, geneticProperties, className, AIHeuristic());
```

The last property should be a value that determines how fit the individual is, where higher is better. For example, AIHeuristic() is a function in the Demo App that returns 0 - the distance from the current color to the desired color. Therefore, closer colors will be considered more fit, and will thus be prioritized once they are sent to the database.

This *does not* save it globally yet. It is cached for the time being, until you send it. The best practice for sending data is whenever you are not as concerned with performance. (For example, at the end of a level) You would call this when an enemy dies, for example, to cache it for now, but not send it yet.

Once you are ready to send, call

```
MongoAI.manager.SendData(className);
```

If you want to delete all global data for your class, call
```
/// This deletes ALL data, so use this only when you want to completely reset your AI
MongoAI.manager.ClearData(className);
```

The reason why every function has a className parameter is so you can separate your parameters for different types of objects. For example, I want to simultaneously run Genetic Algorithms on Boxes and Circles, but I want them to have separate genes, so I use different class names and Stitch will know to put them in different collections.

## Testing for Internet Connection
If you call MongoAI when you have no Internet connection, nothing will happen because you cannot connect to Stitch. Therefore, it may be helpful to validate if you have a connection before you make any requests or saves with MongoAI.

Call the following function if you want to verify that you can send a request to Mongo:
```
MongoAI.manager.HasInternetConnection();
```

If this is false, you will not be able to use MongoAI so you may want to consider how to handle this. It is up to you to decide how to proceed.

# A More Interesting Example: An AI Platformer

In the previous example, we looked at using MongoAI to evolve the color of a box. Now we will examine a more intersting use case. If you look at ```Platformer2DUserControl.cs```, this is the code to control the character in the MongoAI Platformer Demo. In this game, the AI tries to jump over a set of boxes of varying heights and widths.

The AI has 3 choices to make: How fast to run, how hard to jump, and how early before a box to jump. Run the demo project and watch the AI optimize its parameters.

Tests I conducted on this showed that while running 1 instance, the AI took almost 200 seconds to reach the goal.
While running 4 instances, it took almost 50 seconds.
When running 8 instances, it took about 30 seconds.

When looking at the data, it is easy to see how much faster MongoAI can speed up evolution.
