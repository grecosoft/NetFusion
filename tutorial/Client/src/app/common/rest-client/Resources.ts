/**
 * A link with a specified relation-name associated with the resource.
 * Allows for navigation to related resources.
 */
export class Link {
    href: string;
    methods: string[];
    templated: boolean;
    hrefLang?: string;
    name?: string;
    title?: string;
    type?: string;
}

/**
 * Represents a resource containing an associated list of links and 
 * embedded resources.
 */
export interface IHalResource {
    _links: { [id: string]: Link };
    _embedded: { [id:string]: IHalResource };
}

export interface IHalEntryPointResource extends IHalResource {
    version?: string;
}


/**
 * Provides wrapper methods that are implemented by delegating to an IHalResource reference.  
 * This is being done since JS knows nothing about our concrete types.  Instead of having to
 * map every JS return type into its corresponding TypeScript class this class just wraps any
 * IHalResource reference to which it provides helper/utility methods methods. 
 */
export class HalResource {

    private constructor(
        private resource: IHalResource) {

    }

    /**
     * Returns an instance of HalResource for a specific IHalResource returned from
     * the server.  This instance provides method implementations by delegating to 
     * the passed resource.
     * 
     * @param resource Server returned resource conforming to the IHalResource interface.
     */
    public static for(resource: IHalResource): HalResource {
        return new HalResource(resource);
    }

    /**
     * Returns a link with a specified name.
     * 
     * @param name The name of the link to return..
     */
    public getLink(name: string): Link {
        let link = this.resource._links[name];
        
        if (link == null) {

        }
        return link;
    }

    public hasLink(name: string): boolean {
        return !!this.resource._links[name];
    }

    /**
     * Determines if the resource has an embedded resource with a specified name.
     * 
     * @param name The name of the embedded resource assigned by the server.
     */
    public hasEmbedded(name: string): boolean {
        return this.resource._embedded[name] != null;
    }

    /**
     *  Returns an embedded named resource.  If the named resource is not present
     *  an error is thrown.
     * 
     * @param name The name of the embedded resource assigned by the server.
     */
    public getEmbedded<TResource extends IHalResource>(name: string): TResource {
        
        if (!this.hasEmbedded(name)) {

        }

      
        return <TResource>this.resource._embedded[name];
    }

    /**
     * Returns an embedded resource collection.  If the named resource is not present,
     *  an error is thrown.
     * 
     * @param name The name of the embedded resource assigned by the server.
     */
    public getEmbeddedCollection<TResource extends IHalResource>(name: string) : TResource[] {

        if (!this.hasEmbedded(name)) {

        }

        return <any>this.resource._embedded[name];
    }
}