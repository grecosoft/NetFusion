<Query Kind="Program">
  <NuGetReference>NetFusion.Base</NuGetReference>
  <NuGetReference>NetFusion.Bootstrap</NuGetReference>
  <NuGetReference>NetFusion.Common</NuGetReference>
  <NuGetReference>NetFusion.Test</NuGetReference>
  <Namespace>NetFusion.Bootstrap.Configuration</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Exceptions</Namespace>
  <Namespace>NetFusion.Bootstrap.Extensions</Namespace>
  <Namespace>NetFusion.Bootstrap.Logging</Namespace>
  <Namespace>NetFusion.Bootstrap.Manifests</Namespace>
  <Namespace>NetFusion.Bootstrap.Plugins</Namespace>
  <Namespace>NetFusion.Test.Container</Namespace>
  <Namespace>NetFusion.Test.Plugins</Namespace>
  <Namespace>NetFusion.Testing.Logging</Namespace>
</Query>

// The following is an example of the minimal amount of code
// required to bootstrap the NetFusion application container
// for use within LinqPad.

void Main()
{
	// The following four lines simulate the execution environment
	// of a real application host such as a WebApi application.
	var resolver = new TestTypeResolver(this.GetType());
	var hostPlugin = new MockAppHostPlugin();
	
	resolver.AddPlugin(hostPlugin);
	var container = ContainerSetup.Bootstrap(resolver);
	
	// TODO:  Call extension methods to add plug-in components.  
	// This is accomplished in a real host application such as 
	// a WebApi application via type scanning completed by the 
	// non-test type resolver.
	
	// TODO:  Add Container Configurations.
	
	// Build and start the container.
	container.Build();
	container.Start();
	
	// The following can be used to log the details of how the
	// container has been composed:
	container.Log.Dump();
}