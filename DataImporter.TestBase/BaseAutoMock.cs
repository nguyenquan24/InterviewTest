using System.Diagnostics.CodeAnalysis;
using Moq;
using Moq.AutoMock;

namespace DataImporter.TestBase
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseAutoMock<T> where T : class
    {
        private AutoMocker Mocker { get; set; }
        
        protected T ClassUnderTest { get; private set; }

        protected Mock<T> GetMock<T>() where T : class
        {
            if (Mocker == null)
            {
                throw new InvalidOperationException("Mocker has not been initialized. Ensure BaseSetup is called.");
            }
            return Mocker.GetMock<T>();
        }

        [OneTimeSetUp]
        public void BaseSetup()
        {
            Mocker = new AutoMocker();
            InitialiseMocks();
            ClassUnderTest = Mocker.CreateInstance<T>();
        }

        [OneTimeTearDown]
        public void BaseTearDown()
        {
            Mocker = null;
            ClassUnderTest = null;
        }

        protected virtual void InitialiseMocks()
        {
            // Override with mocks that need custom initialisation
        }
    }
}