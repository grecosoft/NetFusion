import { QueryString, QueryInfo } from "./QueryString";
import { RequestHeaders } from "./RequestHeaders";
// import { QueryInfo } from "../queries/QueryInfo";

/**
 * Used to specify settings that should be used when sending a request to the server.
 */
export class RequestSettings {

    public queryString: QueryString;
    public headers: RequestHeaders

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
    public static create(config?: (RequestSettings) => void): RequestSettings {
        let settings = new RequestSettings();

        if (config) {
            config(settings);
        }

        return settings;
    }

     /**
     * Adds request settings commonly used when making request to HAL based services.
     * @returns Reference to instance.
     */
    public useHalDefaults() : RequestSettings {
        this.headers.accept = 'application/hal+json';
        this.headers.contentType = 'application/json';
        return this;
    }

     /**
     * Returns a new RequestSettings instance containing the specified settings
     * merged into the current instance.
     * 
     * @param settingsToMerge The settings to be merged into settings of the current instance.
     * @returns New settings instance containing the merged results.
     */
    public merge(settings: RequestSettings): RequestSettings {
        let mergedSettings = new RequestSettings();
        mergedSettings.headers = this.headers.getMerged(settings.headers);
        mergedSettings.queryString = this.queryString.getMerged(settings.queryString);

        return mergedSettings;
    }

    public populateQueryInfo(query: QueryInfo): RequestSettings {
        this.queryString.addParam('pageIndex', query.pageIndex.toString());
        this.queryString.addParam('pageSize', query.pageSize.toString());
        this.queryString.addParam('sortBy', query.sortBy);
        this.queryString.addParam('sortDirection', query.sortDirection);
        this.queryString.addParams(query.filerBy);

        return this;
    }
}