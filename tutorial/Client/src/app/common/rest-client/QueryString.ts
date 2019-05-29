import { forOwn, clone } from "lodash";
import { MatPaginator, MatSort } from '@angular/material';

/**
 * Settings class used to specify query string parameters.
 */
export class QueryString {
    private values: {[name:string]: string} = {};

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

    public addParams(values: {[name:string]:string}): QueryString {
        forOwn(values, (v, n) => this.values[n] = v)
        return this;
    }

    /**
     * Returns a copy of the query-string name/value pairs.
     */
    public getParams(): {[name:string]:string} {
        return clone(this.values);
    }

    /**
     * Creates a new QueryString instance consisting of the specified settings 
     * merged into the settings of the current instance.
     * 
     * @param queryString The query string settings to be merged.
     */
    public getMerged(queryString: QueryString): QueryString {
        let mergedQuery = new QueryString

        mergedQuery.addParams(this.values);
        mergedQuery.addParams(queryString.values);
        return mergedQuery;
    }
}

export class QueryInfo {
    pageIndex: number;
    pageSize: number;

    sortBy?: string;
    sortDirection?: string;
    filerBy?: {[field: string]: any}   

    public static pageAndSort(paginator: MatPaginator, sort: MatSort): QueryInfo {
        let queryInfo = new QueryInfo();

        queryInfo.pageIndex = paginator.pageIndex || 0;
        queryInfo.pageSize = paginator.pageSize || 10;
        queryInfo.sortBy = sort.active,
        queryInfo.sortDirection = sort.direction;
    
        return queryInfo;
    }

    public static page(paginator: MatPaginator): QueryInfo {
        let queryInfo = new QueryInfo();

        queryInfo.pageIndex = paginator.pageIndex || 0;
        queryInfo.pageSize = paginator.pageSize || 10; 
    
        return queryInfo;
    }
}