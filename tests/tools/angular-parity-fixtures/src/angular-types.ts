/**
 * Type definitions extracted from Angular project's parameters.service.ts
 *
 * These are pure TypeScript interfaces with NO Angular framework dependencies.
 * We extract them here to avoid importing the Angular service which has @Injectable, HttpClient, etc.
 *
 * IMPORTANT: If Angular's types change, update this file to match.
 * Source: external/maas-hesaplama/src/app/core/services/parameters.service.ts
 */

export interface CalculationConstants {
  monthDayCount: number;
  stampTaxRate: number;
  employee: {
    SGKDeductionRate: number;
    SGDPDeductionRate: number;
    unemploymentInsuranceRate: number;
    pensionerUnemploymentInsuranceRate: number;
  };
  employer: {
    SGKDeductionRate: number;
    SGDPDeductionRate: number;
    employerDiscount5746: number;
    unemploymentInsuranceRate: number;
    pensionerUnemploymentInsuranceRate: number;
    SGK5746AdditionalDiscount: number;
  };
}

export interface EmployeeType {
  id: number;
  text: string;
  desc: string;
  order: number;
  show: boolean;
  SGKApplicable: boolean;
  SGKMinWageBasedExemption: boolean;
  SGKMinWageBased17103Exemption: boolean;
  unemploymentInsuranceApplicable: boolean;
  AGIApplicable: boolean;
  incomeTaxApplicable: boolean;
  taxMinWageBasedExemption: boolean;
  researchAndDevelopmentTaxExemption: boolean;
  stampTaxApplicable: boolean;
  employerSGKApplicable: boolean;
  employerUnemploymentInsuranceApplicable: boolean;
  employerIncomeTaxApplicable: boolean;
  employerStampTaxApplicable: boolean;
  employerEducationIncomeTaxExemption: boolean;
  employerSGKShareTotalExemption: boolean;
  employerSGKDiscount5746Applicable: boolean;
  employer5746AdditionalDiscountApplicable: boolean;
}

export interface EmployeeEducationType {
  id: number;
  text: string;
  exemptionRate: number;
}

export interface DisabilityOption {
  id: number;
  degree: number;
  text: string;
}

export interface AGIOption {
  id: number;
  text: string;
  rate: number;
}