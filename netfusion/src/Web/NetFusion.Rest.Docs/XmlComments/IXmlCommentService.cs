using System;
using System.Reflection;
using System.Xml.XPath;

namespace NetFusion.Rest.Docs.XmlComments
{
    /// <summary>
    /// Provides services for reading member comments from an assembly's
    /// XML comment document.
    /// </summary>
    public interface IXmlCommentService
    {
        /// <summary>
        /// Returns a root navigator to an assembly's XML comment document.
        /// </summary>
        /// <param name="containedType">The type contained within the assembly
        /// for which the comments are to be returned.</param>
        /// <returns>XPathNavigator for the root of the XML comments document.</returns>
        XPathNavigator GetXmlCommentsForTypesAssembly(Type containedType);

        /// <summary>
        /// Returns navigator for a type's corresponding XML comment node.
        /// </summary>
        /// <param name="classType">The type for which the XML comment
        /// node is to be returned.</param>
        /// <returns></returns>
        XPathNavigator GetTypeNode(Type classType);

        /// <summary>
        /// Returns comments for a type.
        /// </summary>
        /// <param name="classType">The type for which comments are to be found.</param>
        /// <returns>The comments associated with the type.  If not found, an empty
        /// string is returned.</returns>
        string GetTypeComments(Type classType);

        /// <summary>
        /// Returns the comments for a member of a type.
        /// </summary>
        /// <param name="memberInfo">The type member.</param>
        /// <returns>The comments associated with the type member.  If not found,
        /// an empty string is returned.</returns>
        string GetTypeMemberComments(MemberInfo memberInfo);

        /// <summary>
        /// Returns navigator for a method's corresponding XML document node.
        /// </summary>
        /// <param name="methodInfo">The method for which the XML document
        /// node is to be returned.</param>
        /// <returns>Navigator use to search XML node.</returns>
        XPathNavigator GetMethodNode(MethodInfo methodInfo);

        /// <summary>
        /// Returns the comments for a types method.
        /// </summary>
        /// <param name="methodInfo">The method for which the comments are to be found.</param>
        /// <returns>The comments associated with the method.  If not found, an empty
        /// string is returned.</returns>
        string GetMethodComments(MethodInfo methodInfo);

        /// <summary>
        /// Returns the description for a method's parameter.
        /// </summary>
        /// <param name="methodNode">The XML comment node for the method.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <returns>The comments associated with the method's parameter.  If not
        /// found, an empty string is returned.</returns>
        string GetMethodParamComment(XPathNavigator methodNode, string paramName);
    }
}
