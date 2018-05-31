// Settings used to display confirmation dialog to user.
export class ConfirmSettings {

    constructor(
        public title: string,       
        public message: string) {
    }

    // Text for button used to confirm action.
    public confirmText: string;

    // Text for button used to cancel action.
    public cancelText: string;
} 

// The result of the user's response.
export enum ConfirmResponseTypes {
    ActionConfirmed = 1,
    ActionCanceled = 2
}