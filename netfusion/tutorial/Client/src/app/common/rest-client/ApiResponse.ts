import { HttpResponse } from "@angular/common/http"; 

/**
 * Contains response returned from server.  Contains the
 * HTTP response and the response body as a specific type.
 */
export class ApiResponse<TContent> {
    
    public response: HttpResponse<TContent>;
    public content: TContent; 

    constructor(response: HttpResponse<TContent>) {
        this.response = response;
        this.content = response.body;
    }
}