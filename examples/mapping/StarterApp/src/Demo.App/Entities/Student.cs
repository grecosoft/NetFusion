using NetFusion.Base.Entity;
using System.Collections.Generic;

namespace Demo.App.Entities
{
    public class Student : IAttributedEntity
    {
        private readonly List<int> _scores = new List<int>();

        public string FirstName { get; }
        public string LastName { get; }
        public int[] Scores => _scores.ToArray();

        public bool Passing { get; set; }

        public Student(string firstName, string lastName, IEnumerable<int> scores)
        {
            _scores.AddRange(scores);

            Attributes = new EntityAttributes();
            FirstName = firstName;
            LastName = lastName;
        }

        public IEntityAttributes Attributes { get; }

        public IDictionary<string, object> AttributeValues
        {
            get => Attributes.GetValues();
            set => Attributes.SetValues(value);
        }

        public void AddScore(int score)
        {
            _scores.Add(score);
        }
    }
}
