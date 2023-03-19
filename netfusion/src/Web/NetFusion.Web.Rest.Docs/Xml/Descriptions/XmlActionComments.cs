using System;
using NetFusion.Web.Metadata;
using NetFusion.Web.Rest.Docs.Core.Descriptions;
using NetFusion.Web.Rest.Docs.Models;

namespace NetFusion.Web.Rest.Docs.Xml.Descriptions;

/// <summary>
/// Sets the comments associated with a given Web Controller's action method
/// from a .NET Code Comment XML file.
/// </summary>
public class XmlActionComments : IActionDescription
{
    private readonly IXmlCommentService _xmlComments;

    public XmlActionComments(IXmlCommentService xmlComments)
    {
        _xmlComments = xmlComments ?? throw new ArgumentNullException(nameof(xmlComments));
    }

    public void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
    {
        actionDoc.Description = _xmlComments.GetMethodComments(actionMeta.ActionMethodInfo);
    }
}