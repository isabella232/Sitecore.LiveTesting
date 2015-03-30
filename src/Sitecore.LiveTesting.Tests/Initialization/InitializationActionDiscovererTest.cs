﻿namespace Sitecore.LiveTesting.Tests.Initialization
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Sitecore.LiveTesting.Initialization;
  using Xunit;

  /// <summary>
  /// Defines the test class for <see cref="InitializationActionDiscoverer"/>
  /// </summary>
  public class InitializationActionDiscovererTest
  {
    /// <summary>
    /// Should discover actions by initialization handler attributes.
    /// </summary>
    [Fact]
    public void ShouldDiscoverActionsByInitializationHandlerAttributes()
    {
      const string Argument = "argument";

      InitializationActionDiscoverer actionDiscoverer = new InitializationActionDiscoverer();
      Test test = new Test();

      IEnumerable<InitializationAction> actions = actionDiscoverer.GetInitializationActions(new InitializationContext(test, typeof(Test).GetMethod("TestMethod"), new object[] { Argument })).ToArray();

      Assert.Equal(1, actions.Count());

      InitializationContext expectedInitializationContext = new InitializationContext(test, typeof(Test).GetMethod("TestMethod"), new object[] { Argument });
      
      Assert.Equal(typeof(InitializationHandler1).AssemblyQualifiedName, actions.Single().Id);
      Assert.IsType<object[]>(actions.Single().State);
      Assert.Equal(typeof(InitializationHandler1), ((object[])actions.Single().State)[0]);
      Assert.Equal(new object[] { "parameter" }, ((object[])actions.Single().State)[1]);
      Assert.Equal(expectedInitializationContext.Instance, ((InitializationContext)actions.Single().Context).Instance);
      Assert.Equal(expectedInitializationContext.Method, ((InitializationContext)actions.Single().Context).Method);
      Assert.Equal(expectedInitializationContext.Arguments, ((InitializationContext)actions.Single().Context).Arguments);
    }

    /// <summary>
    /// Should take into the account the priority.
    /// </summary>
    [Fact]
    public void ShouldTakeIntoTheAccoutThePriority()
    {
      InitializationActionDiscoverer actionDiscoverer = new InitializationActionDiscoverer();
      Test test = new Test();

      IEnumerable<InitializationAction> actions = actionDiscoverer.GetInitializationActions(new InitializationContext(test, typeof(Test).GetMethod("TestMethodWithPrioritizedInitializationHandler"), new object[0])).ToArray();

      Assert.Equal(3, actions.Count());
      Assert.Equal(typeof(InitializationHandler2).AssemblyQualifiedName, actions.First().Id);
      Assert.Equal(typeof(InitializationHandler2), ((object[])actions.First().State)[0]);
      Assert.Equal(typeof(InitializationHandler1).AssemblyQualifiedName, actions.ElementAt(1).Id);
      Assert.Equal(typeof(InitializationHandler1), ((object[])actions.ElementAt(1).State)[0]);
      Assert.Equal(typeof(InitializationHandler2).AssemblyQualifiedName, actions.ElementAt(2).Id);
      Assert.Equal(typeof(InitializationHandler2), ((object[])actions.ElementAt(2).State)[0]);
    }

    /// <summary>
    /// Should throw not supported exception if called with other than initialization context type of context.
    /// </summary>
    [Fact]
    public void ShouldThrowNotSupportedExceptionIfCalledWithOtherThanInitializationContextTypeOfContext()
    {
      InitializationActionDiscoverer actionDiscoverer = new InitializationActionDiscoverer();

      Assert.ThrowsDelegate action = () => actionDiscoverer.GetInitializationActions(new object());

      Assert.Throws<NotSupportedException>(action);
    }

    /// <summary>
    /// Defines typical test example.
    /// </summary>
    [InitializationHandler(typeof(InitializationHandler1), "parameter")]
    public class Test : LiveTestWithInitialization
    {
      /// <summary>
      /// Instantiates the test class.
      /// </summary>
      /// <param name="testType">Type of the test class.</param>
      /// <returns>Instance of the test class.</returns>
      public static new LiveTestWithInitialization Instantiate(Type testType)
      {
        return new Test();
      }

      /// <summary>
      /// Sample test method.
      /// </summary>
      public void TestMethod()
      {
      }

      /// <summary>
      /// Another sample test method
      /// </summary>
      [InitializationHandler(typeof(InitializationHandler2), Priority = 100)]
      [InitializationHandler(typeof(InitializationHandler2), Priority = -100)]
      public void TestMethodWithPrioritizedInitializationHandler()
      {
      }
    }

    /// <summary>
    /// Defines sample initialization handler.
    /// </summary>
    public class InitializationHandler1
    {
    }

    /// <summary>
    /// Defines sample initialization handler.
    /// </summary>
    public class InitializationHandler2
    {
    }
  }
}
