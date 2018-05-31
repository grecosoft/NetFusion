import * as _ from 'lodash'

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
    deprecation?: Link;
    profile?: Link;
}

export interface IHalResource {
    _links: { [id: string]: Link };
    _embedded: { [id:string]: IHalResource };

    hasEmbedded(name: string): boolean;
    getEmbedded<TResource extends IHalResource>(name: string): TResource;
    getEmbeddedCollection<TResource extends IHalResource>(name: string) : TResource[];
}

export interface IHalEntryPointResource extends IHalResource {
    version?: string;
}

/**
 *  Representation of a resource returned from the server.
 */
export class HalResource implements IHalResource {
    
    public constructor(jsObj: any) {
        _.extend(this, jsObj);
    }

    /**
     * The links assocated with the resource specified by relation-name.
     */
    public _links: { [id: string]: Link };
    
    /**
     *  Associated embedded resources returned with the resource.
     */
    public _embedded: { [id:string]: any };

    /**
     * Determines if the resource has an embedded resource with a specified name.
     * 
     * @param name The name of the embedded resource assigned by the server.
     */
    public hasEmbedded(name: string): boolean {
        return this._embedded[name] != null;
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

        if ((Object.getPrototypeOf(this._embedded[name]) !== HalResource.prototype))
        {
            this._embedded[name] = new HalResource(this._embedded[name]);
        }
      
        return <TResource>this._embedded[name];
    }

    /**
     * Returns an embedded resource collection.  If the named resource is not present,
     *  an error is thrown.
     * 
     * @param name The name of the embedded resurce assigned by the server.
     */
    public getEmbeddedCollection<TResource extends IHalResource>(name: string) : TResource[] {

        if (!this.hasEmbedded(name)) {

        }

        if ((Object.getPrototypeOf(this._embedded[name]) !== HalResource.prototype))
        {
            this._embedded[name] = new HalResource(this._embedded[name]);
        }
        
        return <TResource[]>this._embedded[name];
    }
}

/**
 * Represents a resource returned from an API service's entry point URL.  This resource will have links 
 * (usually templated) that are the entry URL's for loading resources.  Once a resource is loaded, it
 * links are used for further navigation.
 */
export class HalEntryPointResource extends HalResource
    implements IHalResource {
        
    version?: string;
}



// // Validate and determine how not to have _links and _embedded posted back to server.
// // Raise errors.
// // Validate embedded object is collection.