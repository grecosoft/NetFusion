import { IHalResource } from "./Resources";

/**
 * Represents a resource returned from an API service's entry point URL.  This resource will have links 
 * (usually templated) that are the entry URL's for loading resources.  Once a resource is loaded, it
 * links are used for further navigation.
 */
export interface IHalEntryResource extends IHalResource {
    version?: string;
}