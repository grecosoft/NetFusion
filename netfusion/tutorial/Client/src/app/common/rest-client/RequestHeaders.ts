import {forOwn, clone} from "lodash";

/**
 * Settings class used to specify header values.
 */
export class RequestHeaders {
    private values: {[name:string]: string} = {};

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

    public hasHeader(name: string): boolean {
        return !!this.values[name];
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
    public addHeaders(values: {[name:string]:string}): RequestHeaders {
        forOwn(values, (v, n) => this.values[n] = v);
        return this;
    }

    public removeHeader(name: string) {
        delete this.values[name]
    }

    /**
     * Returns a copy of the header key/value pairs.
     */
    public getHeaders(): {[name:string]:string} {
        return clone(this.values);
    }

    /**
     * Merges the specified request headers into the current instance.
     * and named header values specified on the instance with the same
     * name are overridden.
     * 
     * @param headers Object containing the header values to be merged
     * into the instance.
     */
    public getMerged(headers: RequestHeaders): RequestHeaders {
        var mergedHeaders = new RequestHeaders();

        mergedHeaders.addHeaders(this.values);
        mergedHeaders.addHeaders(headers.values);
        return mergedHeaders;
    }

}