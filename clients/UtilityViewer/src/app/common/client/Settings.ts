import * as _ from 'lodash';

/**
 * Used to specify settings that should be used when sending a request to the server.
 */
export class RequestSettings {
    public queryString: QueryString;
    public headers: RequestHeaders;
    
    public constructor() {
        this.queryString = new QueryString();
        this.headers = new RequestHeaders();
    }

    /**
     * Creates a settings instance used to specify options when making a request.
     * 
     * @param config Optional method passed the created instance for used
     * to specify settings.
     * @returns Created RequestSettings instance.
     */
    public static create(config?: (settings: RequestSettings) => void): RequestSettings {
        var settings = new RequestSettings();
        if (config) {
            config(settings);
        }
        return settings;
    }

    /**
     * Adds request settings commonly used when making request to HAL based services.
     */
    public useHalDefaults() {

        this.headers.accept = 'application/hal+json';
        this.headers.contentType = 'application/json';
    }

    /**
     * Returns a new RequestSettings instance containing the specified settings
     * merged into the current instance.
     * 
     * @param settingsToMerge The settings to be merged into settings of the current instance.
     * @returns New settings instance containing the merged results.
     */
    public merge(settingsToMerge: RequestSettings): RequestSettings {
        if (!settingsToMerge) return this;

        return <RequestSettings>{
            headers: this.headers.getMerged(settingsToMerge.headers),
            queryString: this.queryString.getMerged(settingsToMerge.queryString)};
    } 
}

/**
 * Settings class used to specify query string parameters.
 */
export class QueryString {
    private values: {[name: string]: string } = {};

    /**
     * Adds a query string parameter to the settings.
     * 
     * @param name The name of the query string parameter.
     * @param value The value of the query string parameter.
     */
    public addParam(name: string, value: string): QueryString {
        this.values[name] = value;
        return this;
    }

    /**
     * Returns a copy of the query-string name/value pairs.
     */
    public getParams(): {[name: string]:string} {
        return _.clone(this.values);
    }

    /**
     * Creates a new QueryString instance consisting of the specified settings 
     * merged into the settings of the current instance.
     * 
     * @param queryString The query string settings to be merged.
     */
    public getMerged(queryString: QueryString): QueryString {
        let mergedQuery = new QueryString();

        _.forOwn(this.values, (value: string, name: string) => {
            mergedQuery[name] = value;
        });

        _.forOwn(queryString.values, (value: string, name: string) => {
            mergedQuery[name] = value;
        });
        
        return mergedQuery;
    }
}

/**
 * Settings class used to specify header values.
 */
export class RequestHeaders {
    private values: {[name: string]:string} = {};

    /**
     * The specified accept-type. 
     */
    get accept(): string {
        return this.values['Accept'];
    }

    /**
     * Sets the specified accept-type. 
     */
    set accept(value: string) {
        this.values['Accept'] = value;
    }

    /**
     * The specified content-type. 
     */
    get contentType(): string {
        return this.values['Content-Type'];
    }

    /**
     * Sets the specified content-type. 
     */
    set contentType(value: string) {
        this.values['Content-Type'] = value;
    }

    /**
     * Adds a header key/value pair.
     * 
     * @param name The name of the header.
     * @param value The value of the header.
     */
    public addHeader(name: string, value: string): RequestHeaders {
        this.values[name] = value;
        return this;
    }

     /**
     * Adds a multiple header key/value pairs.
     * 
     * @param headers Object containing the header values.
     */
    public addHeaders(headers: {[name: string]:string}): RequestHeaders {
        _.forOwn(headers, (value: string, name: string) => {
            this.values[name] = value;
        });
        return this;
    }

    /**
     * Returns a copy of the header key/value pairs.
     */
    public getHeaders(): {[name: string]:any} {
        return _.clone(this.values);
    }

      /**
     * Adds a multiple header key/value pairs.
     * 
     * @param headers Object containing the header values.
     */
    public getMerged(headers: RequestHeaders): RequestHeaders {
        var mergedHeaders = new RequestHeaders();
        
        _.forOwn(this.values, (value: string, name: string) => {
            mergedHeaders.values[name] = value;
        });

        _.forOwn(headers.values, (value: string, name: string) => {
            mergedHeaders.values[name] = value;
        });
        return mergedHeaders;
    }
}