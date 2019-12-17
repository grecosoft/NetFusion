using Demo.Domain.Entities;
using System.Collections.Generic;
using NetFusion.Base.Scripting;

namespace Demo.Domain.Scripts
{
    public static class StudentScripts
    {
        public static EntityScript[] GetScripts(bool setIdentityValue = true)
        {
            var scoreScriptExpressions = new List<EntityExpression>
            {
                new EntityExpression("Entity.Scores.Min()", 0, "MinScore"),
                new EntityExpression("Entity.Scores.Max()", 1, "MaxScore"),
                new EntityExpression("Entity.Scores.Sum()/Entity.Scores.Count()", 2, "AverageScore"),
                new EntityExpression("_.MaxScore - _.MinScore", 3, "Difference"),
                new EntityExpression("Entity.Passing = _.AverageScore >= _.PassingScore", 4)
            };

            var displayScriptExpressions = new List<EntityExpression>
            {
                new EntityExpression("$\"{Entity.FirstName} - {Entity.LastName}\"", 0, "DisplayName")
            };

            var displayScript = new EntityScript(
                setIdentityValue ? "38F9560F-A8E4-4A64-81A6-77C66FA927C9" : null,
                "default",
                typeof(Student).AssemblyQualifiedName,
                displayScriptExpressions.AsReadOnly());

            displayScript.ImportedNamespaces.Add("System");

            var calcScript = new EntityScript(
                setIdentityValue ? "B83FD639-4AAC-4CBC-AA17-645DEAC4147B" : null,
                "scoreCalcs",
                typeof(Student).AssemblyQualifiedName,
                scoreScriptExpressions.AsReadOnly());

            calcScript.InitialAttributes["PassingScore"] = 70;

            calcScript.ImportedNamespaces.Add("System");
            calcScript.ImportedNamespaces.Add("System.Linq");

            return new[] { calcScript, displayScript };
        }
    }
}