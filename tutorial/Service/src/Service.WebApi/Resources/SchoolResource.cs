using NetFusion.Rest.Resources;

namespace Service.WebApi.Resources
{
    /// <summary>
    /// Contains information about a school.
    /// </summary>
    [Resource("type-school")]
    public class SchoolResource 
    {
        /// <summary>
        /// Identities the school.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The registered name of the school.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The number of teacher employed by the school.
        /// </summary>
        public int NumberTeachers { get; set; }

        /// <summary>
        /// The year the school was registered.
        /// </summary>
        public int YearEstablished { get; set; }
    }
}