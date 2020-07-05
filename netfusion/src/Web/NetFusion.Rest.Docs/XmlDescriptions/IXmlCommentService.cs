using System;
using System.Reflection;
using System.Xml.XPath;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    /// <summary>
    /// Provides services for reading member comments from an assembly's
    /// XML comment document.
    /// </summary>
    public interface IXmlCommentService
    {
        /// <summary>
        /// Returns a root navigator to an assembly's XML document document.
        /// </summary>
        /// <param name="containedType">The type contained within the assembly
        /// for which the comments are to be returned.</param>
        /// <returns>XPathNavigator for the root of the XML comments document.</returns>
        XPathNavigator GetXmlCommentsForTypesAssembly(Type containedType);

        /// <summary>
        /// Returns the summary for a types method.
        /// </summary>
        /// <param name="methodInfo">The method for which the comments are to be found.</param>
        /// <returns>The comments associated with the method.  If not found, an empty
        /// string is returned.</returns>
        string GetMethodSummary(MethodInfo methodInfo);

        /// <summary>
        /// Returns navigator for a method's corresponding XML document node.
        /// </summary>
        /// <param name="methodInfo">The method for which the XML document
        /// node is to be returned.</param>
        /// <returns>Navigator use to search XML node.</returns>
        XPathNavigator GetMethodNode(MethodInfo methodInfo);

        /// <summary>
        /// Returns the description for a method's parameter.
        /// </summary>
        /// <param name="methodNode">The XML comment note for the method.</param>
        /// <param name="paramName">The paramater name.</param>
        /// <returns>The comments assocated with the method's parameter.  If not
        /// found, an empty string is returned.</returns>
        string GetMethodParamComment(XPathNavigator methodNode, string paramName);

    }
}
