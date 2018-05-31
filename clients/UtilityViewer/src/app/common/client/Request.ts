import { Link, HalResource } from './Resource'
import { RequestSettings } from './Settings'
import * as _ from 'lodash'
import { HttpResponse } from '@angular/common/http/src/response';

/**
 * Class used to specify a request to be made to HAL based services.
 */
export class ApiRequest {
    public requestUri: string;
    public method: string;
    public isTemplate: boolean;
    public embeddedNames: string;
    public content: any;
    public settings: RequestSettings;

    /**
     * Creates a new request.
     *
     * @param requestUri The URI to request.  Appended to the configured baseAddress.
     * @param method The HTTP method used to invoke URI.
     * @param config Optional delegate used to specify additional configuration.
     * @returns The created ApiRequest instance.
     */
    public static create(requestUri: string, method: string, config?: (ApiRequest) => void): ApiRequest {

        let request = new ApiRequest();
        request.requestUri = requestUri;
        request.method = method;
        request.isTemplate = ApiRequest.isTemplateUrl(requestUri);

        if (config) {
            config(request);
        }

        return request;
    }

    //------------------ REQUEST CREATION METHODS -------------------------

    /**
     * Creates a new GET request.
     *
     * @param requestUri The URI to request.  Appended to the configured baseAddress.
     * @param config Optional delegate used to specify additional configuration.
     * @returns The created ApiRequest instance.
     */
    public static get(requestUri: string, config?: (ApiRequest) => void): ApiRequest {

        return ApiRequest.create(requestUri, "GET", config);
    }

    /**
     * Creates a new POST request.
     *
     * @param requestUri The URI to request.  Appended to the configured baseAddress.
     * @param config Optional delegate used to specify additional configuration.
     * @returns The created ApiRequest instance.
     */
    public static post(requestUri: string, config?: (ApiRequest) => void): ApiRequest {
        return ApiRequest.create(requestUri, "POST", config);
    }

    /**
     * Creates a new PUT request.
     *
     * @param requestUri The URI to request.  Appended to the configured baseAddress.
     * @param config Optional delegate used to specify additional configuration.
     * @returns The created ApiRequest instance.
     */
    public static put(requestUri: string, config?: (ApiRequest) => void): ApiRequest {
        return ApiRequest.create(requestUri, "PUT", config)
    }

    /**
     * Creates a new DELETE request.
     *
     * @param requestUri The URI to request.  Appended to the configured baseAddress.
     * @param config Optional delegate used to specify additional configuration.
     * @returns The created ApiRequest instance.
     */
    public static delete(requestUri: string, config?: (ApiRequest) => void): ApiRequest {
        return ApiRequest.create(requestUri, "DELETE", config);
    }

    /**
     * Creates a new request from a resource link relation.
     *
     * @param link The link used to create request.  The link's URI is appended to the configured baseAddress.
     * @param config Optional delegate used to specify additional configuration.
     * @returns The created ApiRequest instance.
     */
    public static fromLink(link: Link, config?: (request: ApiRequest) => void): ApiRequest {
        return ApiRequest.create(link.href, link.methods[0], config);
    }

    //------------------ REQUEST OPTIONS ---------------------------

    /**
     * Populates an URI template with a set of name/value pairs.
     *
     * @param tokens Dictionary of name/value pairs.
     * @returns The ApiRequest instance.
     */
    public withRouteValues(tokens: {[name: string]: any}): ApiRequest {
        this.requestUri = this.replaceTemplateTokensWithValues(this.requestUri, tokens);
        this.isTemplate = ApiRequest.isTemplateUrl(this.requestUri);

        return this;
    }

    /**
     * Provides the server with a hint as to what embedded resources should
     * be returned.  This allows less data to be returned to the client.  
     * This is an optional feature and may not be implemented by the API.
     *
     * @param names Embedded list of names.
     * @returns The ApiRequest instance.
     */
    public embed(...names: string[]): ApiRequest {
        this.embeddedNames = _.join(name, ',');
        return this;
    }

    /**
     * Set the body of the request.
     *
     * @param content The content to be sent as the body of the request.
     * @returns The ApiRequest instance.
     */
    public withContent(content: any): ApiRequest {
        this.content = content;
        return this;
    }

    /**
     * Specifies the request specific settings. These settings are merged
     * into the default settings.
     *
     * @param names Settings specific to the request.
     * @returns The ApiRequest instance.
     */
    public usingSettings(settings: RequestSettings): ApiRequest {
        this.settings = settings;
        return this;
    }

    //------------------ URL TEMPLATE METHODS ---------------------------

    private replaceTemplateTokensWithValues(urlTemplate: string, tokens: {[name: string]:any} ): string {
        
        _.forOwn(tokens, (value: any, key: string) => {
            urlTemplate = this.replaceRouteTokens(urlTemplate, key, value);
            urlTemplate = this.replaceRouteTokens(urlTemplate, _.lowerFirst(key), value);
        });

        return this.removeOptionalRouteTokens(urlTemplate);
    }

    private replaceRouteTokens(urlTemplate: string, routeKey: string, routeValue: any): string {
        let url = _.replace(urlTemplate, '{' + routeKey + '}', routeValue);
        return _.replace(url, '{?' + '}',  routeValue)        
    }

    private static isTemplateUrl(url: string): boolean {
        return url.indexOf('/{') !== -1;
    }

    private removeOptionalRouteTokens(populatedUrl: string): string {
        return _.split(populatedUrl, '/{?')[0];
    }
}

export class ApiResponse<TContent> {
    
    public response: HttpResponse<TContent>;
    public content: TContent; 

    constructor(response:any) {
        this.response = response;
        this.content = <any>new HalResource(response.body);
    }
}

