# Create CLI's with incredible ease

Yeah, it is pretty old school using the full .NET framework and MEF to create CLI's with incredible ease.

Just Create a Console App using the Visual Studio template. Add The Nuget package (not published but will be shortly)

And create an implementation of the abstract class CLIProvider for each verb in your application.

Lets say you wanted to create `MYGIT COMMIT`, `MYGIT CHECKOUT` and `MYGIT BRANCH` this would be three seperate implementations of CLIProvider.

E.g: MYGIT Checkout would be

``` csharp
    public class CheckoutProvider : CLIProvider
    {
        public override string Verb => "Checkout";
        public override bool HasVerbArgument => false;

        protected override void Execute()
        {
        	//Do the stuff you need to execute when calling checkout
        }
    }
```

Automagically you would get a Help text when running MYGIT with the wrong parameters, or MYGIT with help as the first parameter.

Also you would automatically get a help text for checkout when running MYGIT Checkout help

Now an actual Git checkout would need a branch name as a verbargument. This is implemented like this:


``` csharp
    public class CheckoutProvider : CLIProvider
    {
        public override string Verb => "Checkout";
        public override bool HasVerbArgument => true;

        protected override void Execute()
        {
        	string branchName = VerbArgument;
        }
    }
```

When running MYGIT Checkout MyBranch - the VerbArgument will contain the word "MyBranch"

If you need more parameters you do this to add as many as you like in the SetupParameters list

``` csharp
    public class CheckoutProvider : CLIProvider
    {
        public override string Verb => "Checkout";
        public override bool HasVerbArgument => true;

        public override List<Parameter> SetupParameters => new List<Parameter>
        {
            new Parameter("v", "Verbose", typeof(bool), "Makes the output verbose"),
            new Parameter("tp", "Testprint", typeof(string), "Prints what ever value is provided to testprint")
        };

        protected override void Execute()
        {
        	string branchName = VerbArgument;
        	var verbose = Parameters.Verbose;
        	Console.WriteLine(Parameters.TestPrint);
        }
    }
```

Now you can read the extra Parameters using the Parameters property of the CLIProvider. It is actually a dynamic and the parameter given to the Command is automatically injected. A bool parameter is false by default and set to true if used. The above would be called like this: `MYGIT checkout MyBranch -v` or `MYGIT checkout MyBranch -verbose`.

The TestPrint would be called like this `MYGIT checkout MyBranch -testprint='Testing'`. which would print `Testing`  to the console in the example above.

There are also automatic AppSetting and ConnectionString settings just like the Parameter one as shown here:

``` csharp
    public class CheckoutProvider : CLIProvider
    {
        public override string Verb => "Checkout";
        public override bool HasVerbArgument => true;

        public override List<AppSetting> SetupAppSettings => new List<AppSetting>
        {
            new AppSetting("Production", typeof(bool), "Is this production")
        };

        public override List<ConnectionString> SetupConnectionStrings => new List<ConnectionString>
        {
            new ConnectionString("TheDatabase", "Connectionstring to the database")
        };

        public override List<Parameter> SetupParameters => new List<Parameter>
        {
            new Parameter("v", "Verbose", typeof(bool), "Makes the output verbose"),
            new Parameter("tp", "Testprint", typeof(string), "Prints what ever value is provided to testprint")
        };

        protected override void Execute()
        {
        	string branchName = VerbArgument;
        	var verbose = Parameters.Verbose;
        	Console.WriteLine(Parameters.TestPrint);
        	Console.WriteLine(AppSettings.Production);
        	Console.WriteLine(ConnectionStrings.TheDatabase);
        }
    }
```

I am sure you will agree this is as easy as it gets to create a Windows CLI.

As it uses MEF you can either create the Verb classes in the Console App or in one or more seperate class libraries. They will be loaded on-the-fly no matter where they are.