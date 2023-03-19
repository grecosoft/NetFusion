namespace NetFusion.Web.Common;

/// <summary>
/// Common Resource Relation Types.
/// </summary>
public static class RelationTypes
{
    public static class Editing
    {
        /// <summary>
        /// The target IRI points to a resource where a submission form can be obtained.
        /// </summary>
        public const string CreateForm = "create-form";

        /// <summary>
        /// Refers to a resource that can be used to edit the link's context.
        /// </summary>
        public const string Edit = "edit";

        /// <summary>
        /// The target IRI points to a resource where a submission form for editing associated resource can be obtained.
        /// </summary>
        public const string EditForm = "edit-form";
    }

    public static class History
    {
        /// <summary>
        /// Points to a resource containing the latest (e.g., current) version of the context.
        /// </summary>
        public const string LatestVersion = "latest-version";

        /// <summary>
        /// Points to a resource containing the successor version in the version history.
        /// </summary>
        public const string SuccessorVersion = "successor-version";

        /// <summary>
        /// Points to a resource containing the version history for the context.
        /// </summary>
        public const string VersionHistory = "version-history";

        /// <summary>
        /// Points to a working copy for this resource.
        /// </summary>
        public const string WorkingCopy = "working-copy";

        /// <summary>
        /// Points to the versioned resource from which this working copy was obtained.
        /// </summary>
        public const string WorkingCopyOf = "working-copy-of";

        /// <summary>
        /// Refers to the immediately following archive resource
        /// </summary>
        public const string NextArchive = "next-archive";

        /// <summary>
        /// Refers to a collection of records, documents, or other materials of historical interest.
        /// </summary>
        public const string Archives = "archives";

        /// <summary>
        /// The document linked to was later converted to the document that contains this link relation. For example, 
        /// an RFC can have a link to the Internet-Draft that became the RFC; in that case, the link relation would 
        /// be "convertedFrom".
        /// </summary>
        public const string ConvertedFrom = "convertedFrom";

        /// <summary>
        /// Refers to the immediately preceding archive resource.
        /// </summary>
        public const string PreArchive = "pre-archive";

        /// <summary>
        /// The Target IRI points to a Memento, a fixed resource that will not change state anymore
        /// </summary>
        public const string Memento = "memento";
    }
        
    public static class Navigation
    {
        /// <summary>
        /// An IRI that refers to the furthest preceding resource in a series of resources.
        /// </summary>
        public const string First = "first";

        /// <summary>
        /// An IRI that refers to the furthest following resource in a series of resources.
        /// </summary>
        public const string Last = "last";

        /// <summary>
        /// Indicates that the link's context is a part of a series, and that the next in the series is the link target.
        /// </summary>
        public const string Next = "next";

        /// <summary>
        /// Indicates that the link's context is a part of a series, and that the previous in the series is the link target.
        /// </summary>
        public const string Prev = "prev";

        /// <summary>
        /// Refers to the first resource in a collection of resources.
        /// </summary>
        public const string Start = "start";

        /// <summary>
        /// Refers to a parent document in a hierarchy of documents.
        /// </summary>
        public const string Up = "up";

        /// <summary>
        /// Points to a resource containing the predecessor version in the version history.
        /// </summary>
        public const string PredecessorVersion = "predecessor-version";

        /// <summary>
        /// Refers to the previous resource in an ordered series of resources. Synonym for "prev".
        /// </summary>
        public const string Previous = "previous";
    }

    public static class Reference
    {
        /// <summary>
        /// Refers to a resource that is the subject of the link's context.
        /// </summary>
        public const string About = "about";

        /// <summary>
        /// Refers to the context's author.
        /// </summary>
        public const string Author = "author";

        /// <summary>
        /// Refers to a copyright statement that applies to the link's context.
        /// </summary>
        public const string Copyright = "copyright";

        /// <summary>
        /// Refers to context-sensitive help.
        /// </summary>
        public const string Help = "help";

        /// <summary>
        /// Refers to an icon representing the link's context.
        /// </summary>
        public const string Icon = "icon";

        /// <summary>
        /// Refers to a glossary of terms.
        /// </summary>
        public const string Glossary = "glossary";
    }

    public static class Monitoring
    {
        /// <summary>
        /// Refers to a hub that enables registration for notification of updates to the context.
        /// </summary>
        public const string Hub = "hub";

        /// <summary>
        /// Refers to a resource that can be used to monitor changes in an HTTP resource.
        /// </summary>
        public const string Monitor = "monitor";

        /// <summary>
        /// Refers to a resource that can be used to monitor changes in a specified group of HTTP resources
        /// </summary>
        public const string MonitorGroup = "monitor-group";
    }

    /// <summary>
    /// Refers to a substitute for this context
    /// </summary>
    public const string Alternate = "alternate";

    /// <summary>
    /// Refers to a resource providing information about the link's context.
    /// </summary>
    public const string DescribedBy = "describedby";

    /// <summary>
    /// The relationship A 'describes' B asserts that resource A provides a description of resource B. There are no 
    /// constraints on the format or representation of either A or B, neither are there any further constraints on 
    /// either resource.
    /// </summary>
    public const string Describes = "describes";

    /// <summary>
    /// The target IRI points to a resource which represents the collection resource for the context IRI.
    /// </summary>
    public const string Collection = "collection";

    /// <summary>
    /// Refers to a resource containing the most recent item(s) in a collection of resources.
    /// </summary>
    public const string Current = "current";

    /// <summary>
    /// The Target IRI points to an Original Resource.
    /// </summary>
    public const string Original = "original";

    /// <summary>
    /// The prefetch link relation type is used to identify a resource that might be required by the next navigation 
    /// from the link context, and that the user agent ought to fetch, such that the user agent can deliver a faster
    /// response once the resource is requested in the future.
    /// </summary>
    public const string Prefetch = "prefetch";

    /// <summary>
    /// Refers to a resource that provides a preview of the link's context.
    /// </summary>
    public const string Preview = "preview";

    /// <summary>
    /// Identifies a related resource.
    /// </summary>
    public const string Related = "related";

    /// <summary>
    /// Identifies a resource that is a reply to the context of the link.
    /// </summary>
    public const string Replies = "replies";

    /// <summary>
    /// Conveys an identifier for the link's context.
    /// </summary>
    public const string Self = "self";
}