﻿namespace Sitecore.LiveTesting
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Web.Hosting;

  /// <summary>
  /// Defines the class that creates and manages applications from <see cref="ApplicationHost"/>.
  /// </summary>
  /// <typeparam name="T">Type of the application object to create remotely.</typeparam>
  public class TestApplicationManager<T> where T : TestApplication, new()
  {
    /// <summary>
    /// The application manager.
    /// </summary>
    private readonly ApplicationManager applicationManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestApplicationManager{T}"/> class.
    /// </summary>
    public TestApplicationManager() : this(ApplicationManager.GetApplicationManager())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestApplicationManager{T}"/> class.
    /// </summary>
    /// <param name="applicationManager">The application manager.</param>
    public TestApplicationManager(ApplicationManager applicationManager)
    {
      if (applicationManager == null)
      {
        throw new ArgumentNullException("applicationManager");
      }

      this.applicationManager = applicationManager;
    }

    /// <summary>
    /// Gets the application manager.
    /// </summary>
    protected ApplicationManager ApplicationManager
    {
      get { return this.applicationManager; }
    }

    /// <summary>
    /// Starts application from the application host definition.
    /// </summary>
    /// <param name="applicationHost">The application host.</param>
    /// <returns>Instance of test application.</returns>
    public virtual T StartApplication(ApplicationHost applicationHost)
    {
      if (applicationHost == null)
      {
        throw new ArgumentNullException("applicationHost");
      }

      return (T)this.ApplicationManager.CreateObject(applicationHost.ApplicationId, typeof(T), applicationHost.VirtualPath, Path.GetFullPath(applicationHost.PhysicalPath), false, true);
    }

    /// <summary>
    /// Gets the running application that corresponds to the provided application host.
    /// </summary>
    /// <param name="applicationHost">The application host.</param>
    /// <returns>Matching application instance.</returns>
    public virtual TestApplication GetRunningApplication(ApplicationHost applicationHost)
    {
      if (applicationHost == null)
      {
        throw new ArgumentNullException("applicationHost");
      }

      return (T)this.ApplicationManager.GetObject(applicationHost.ApplicationId, typeof(T));
    }

    /// <summary>
    /// Stops the application by it's application host definition.
    /// </summary>
    /// <param name="application">The test application.</param>
    public virtual void StopApplication(TestApplication application)
    {
      if (application == null)
      {
        throw new ArgumentNullException("application");
      }

      this.ApplicationManager.ShutdownApplication(application.Id);
    }

    /// <summary>
    /// Gets the list of running applications.
    /// </summary>
    /// <returns>The list of running applications.</returns>
    public IEnumerable<TestApplication> GetRunningApplications()
    {
      List<TestApplication> result = new List<TestApplication>();

      foreach (ApplicationInfo application in this.applicationManager.GetRunningApplications())
      {
        TestApplication candidate = this.applicationManager.GetObject(application.ID, typeof(T)) as TestApplication;

        if (candidate != null)
        {
          result.Add(candidate);
        }
      }

      return result;
    }
  }
}