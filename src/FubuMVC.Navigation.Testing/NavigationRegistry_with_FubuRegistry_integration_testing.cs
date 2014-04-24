using System.Linq;
using FubuLocalization;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using NUnit.Framework;

namespace FubuMVC.Navigation.Testing
{
    [TestFixture]
    public class NavigationRegistry_with_FubuRegistry_integration_testing
    {
        private FubuRegistry registry;
        private FubuRuntime runtime;

        [SetUp]
        public void SetUp()
        {
            registry = new FubuRegistry();
            runtime = null;
        }

        [TearDown]
        public void TearDown()
        {
            if (runtime != null) runtime.Dispose();
        }

        private IMenuResolver resolver
        {
            get
            {
                if (runtime == null)
                {
                    runtime = FubuApplication.For(registry).StructureMap().Bootstrap();
                }

                return runtime.Factory.Get<IMenuResolver>();
            }
        }

        [Test]
        public void navigation_method_on_fubu_registry_works()
        {
            registry.Import<NavigationRegistryExtension>();

            registry.Policies.Global.Add<NavigationRegistry>(x => {
                x.ForMenu(FakeKeys.Key1);
                x.Add += MenuNode.Node(FakeKeys.Key2);
                x.Add += MenuNode.Node(FakeKeys.Key3);
            });

            resolver.MenuFor(FakeKeys.Key1).Select(x => x.Key)
                .ShouldHaveTheSameElementsAs(FakeKeys.Key2, FakeKeys.Key3);
        }

        [Test]
        public void import_navigation_from_child_registry()
        {
            registry.Import<NavigationRegistryExtension>();

            registry.Policies.Global.Add<NavigationRegistry>(x => {
                x.ForMenu(FakeKeys.Key1);
                x.Add += MenuNode.Node(FakeKeys.Key2);
                x.Add += MenuNode.Node(FakeKeys.Key3);
            });

            registry.Import<ChildRegistry>();


            resolver.MenuFor(FakeKeys.Key1).Select(x => x.Key)
                .ShouldHaveTheSameElementsAs(FakeKeys.Key2, FakeKeys.Key3, FakeKeys.Key4, FakeKeys.Key5);

            resolver.MenuFor(FakeKeys.Key6).Select(x => x.Key)
                .ShouldHaveTheSameElementsAs(FakeKeys.Key7, FakeKeys.Key8);
        }

        [Test]
        public void import_with_strings_instead_of_StringToken()
        {
            registry.Import<NavigationRegistryExtension>();

            registry.Policies.Global.Add<NavigationRegistry>(x => {
                x.ForMenu("Key1");
                x.Add += MenuNode.Node("Key2");
                x.Add += MenuNode.Node("Key3");
            });

            registry.Import<ChildRegistry>();


            resolver.MenuFor(new NavigationKey("Key1")).Select(x => x.Key)
                .ShouldHaveTheSameElementsAs(new NavigationKey("Key2"), new NavigationKey("Key3"));
        }

        public class ChildRegistry : FubuPackageRegistry
        {
            public ChildRegistry()
            {
                Policies.Global.Add<ChildNavigation>();
            }
        }

        public class ChildNavigation : NavigationRegistry
        {
            public ChildNavigation()
            {
                ForMenu(FakeKeys.Key1);
                Add += MenuNode.Node(FakeKeys.Key4);
                Add += MenuNode.Node(FakeKeys.Key5);

                ForMenu(FakeKeys.Key6);
                Add += MenuNode.Node(FakeKeys.Key7);
                Add += MenuNode.Node(FakeKeys.Key8);
            }
        }
    }
}