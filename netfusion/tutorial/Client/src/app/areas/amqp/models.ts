export class CreateClaimSubmission {
    insuredId: string;
    insuredFirstName: string;
    insuredLastName: string;
    insuredDeductible: number;
    claimEstimate: number;
    claimDescription: string;
}

export class ClaimStatusUpdated {
    insuredId: string;
    currentState: string;
    nextStatusUpdate: string;
    nextStatus: string;
}