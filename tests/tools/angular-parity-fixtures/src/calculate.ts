#!/usr/bin/env npx ts-node
/**
 * Angular Salary Calculator CLI
 *
 * A pure CLI tool that wraps Angular's salary calculator for parity testing.
 * Accepts JSON input via stdin, outputs JSON result to stdout.
 *
 * Usage:
 *   echo '{"year":2026,"calculationMode":"GROSS_TO_NET",...}' | npx ts-node src/calculate.ts
 *
 * Exit codes:
 *   0 - Success
 *   1 - Validation error (invalid input)
 *   2 - Calculation error (runtime error)
 *   3 - Internal error (unexpected)
 */

import * as path from 'path';
import * as readline from 'readline';

// ============================================================================
// Types
// ============================================================================

interface CalculationInput {
  year: number;
  calculationMode: 'GROSS_TO_NET' | 'NET_TO_GROSS' | 'TOTAL_TO_GROSS';
  employeeTypeId: number;
  salaryAmount: number;
  workedDays: number;
  researchAndDevelopmentWorkedDays: number;
  disabilityDegree: number;
  agiRate: number;
  educationExemptionRate: number;
  isPensioner: boolean;
  applyEmployerDiscount5746: boolean;
  isAgiIncludedNet: boolean;
  isAgiIncludedTax: boolean;
  applyMinWageTaxExemption: boolean;
  isAgiCalculationEnabled: boolean;
}

interface MonthOutput {
  monthNumber: number;
  calculatedGrossSalary: number;
  sgkBase: number;
  incomeTaxBase: number;
  cumulativeIncomeTaxBase: number;
  employeeSgkDeduction: number;
  employeeSgkExemption: number;
  employeeFinalSgkDeduction: number;
  employeeUnemploymentInsuranceDeduction: number;
  employeeUnemploymentInsuranceExemption: number;
  employeeFinalUnemploymentInsuranceDeduction: number;
  employeeIncomeTax: number;
  employeeIncomeTaxExemption: number;
  stampTax: number;
  employeeStampTaxExemption: number;
  employerStampTaxExemption: number;
  totalStampTaxExemption: number;
  employerStampTax: number;
  netSalary: number;
  agiAmount: number;
  finalNetSalary: number;
  employerSgkDeduction: number;
  employerSgkExemption: number;
  employerFinalSgkDeduction: number;
  employerUnemploymentInsuranceDeduction: number;
  employerUnemploymentInsuranceExemption: number;
  employerFinalUnemploymentInsuranceDeduction: number;
  employerIncomeTaxExemption: number;
  employerFinalIncomeTax: number;
  employerTotalSgkCost: number;
  employerTotalCost: number;
  totalSgkExemption: number;
  employeeMinWageTaxExemption: number;
}

interface TotalsOutput {
  calculatedGrossSalary: number;
  netSalary: number;
  finalNetSalary: number;
  employerTotalCost: number;
  totalSgkExemption: number;
}

interface CalculationOutput {
  months: MonthOutput[];
  totals: TotalsOutput;
}

interface SuccessResponse {
  success: true;
  input: CalculationInput;
  output: CalculationOutput;
  calculatedAt: string;
}

interface ErrorResponse {
  success: false;
  error: {
    code: 'VALIDATION_ERROR' | 'CALCULATION_ERROR' | 'INTERNAL_ERROR';
    message: string;
    details?: string[];
  };
}

type CliResponse = SuccessResponse | ErrorResponse;

// ============================================================================
// Angular Model Imports (same as generate-fixtures.ts)
// ============================================================================

const angularModelsPath = path.resolve(__dirname, '../../../../external/maas-hesaplama/src/app/core/models');
const angularBasePath = path.resolve(__dirname, '../../../../external/maas-hesaplama/src');

// Dynamic imports to avoid rootDir issues
const { MonthCalculationModel } = require(path.join(angularModelsPath, 'month-calculation.model'));
const { YearCalculationModel } = require(path.join(angularModelsPath, 'year-calculation.model'));
const {
  YearDataModel,
  TaxSliceModel,
  MinGrossWage,
  DisabledMonthlyIncomeTaxDiscountBaseModel
} = require(path.join(angularModelsPath, 'year-data.model'));

// Load Angular's static data
const fixturesJson = require(path.join(angularBasePath, 'assets/fixtures.json'));
const yearParametersJson = require(path.join(angularBasePath, 'assets/year-parameters.json'));

// Extract constants
const MONTHS: string[] = fixturesJson.MONTHS;
const CALCULATION_CONSTANTS = fixturesJson.CALCULATION_CONSTANTS;
const EMPLOYEE_TYPES = fixturesJson.EMPLOYEE_TYPES.options;
const EMPLOYEE_EDUCATION_TYPES = fixturesJson.EMPLOYEE_EDUCATION_TYPES.options;
const DISABILITY_OPTIONS = fixturesJson.DISABILITY_OPTIONS.options;
const AGI_OPTIONS = fixturesJson.AGI_OPTIONS.options;

// Parse year parameters (cached)
const YEAR_PARAMETERS = parseYearParameters();

function parseYearParameters(): any[] {
  const params: any[] = [];

  for (const item of yearParametersJson.yearParameters) {
    const taxSlices = item.taxSlices.map(
      (ts: { rate: number; ceil: number }) => new TaxSliceModel(ts.rate, ts.ceil)
    );

    const disabledBases = item.disabledMonthlyIncomeTaxDiscountBases.map(
      (d: { degree: number; amount: number }) =>
        new DisabledMonthlyIncomeTaxDiscountBaseModel(d.degree, d.amount)
    );

    const sortedMinWages = [...item.minGrossWages].sort(
      (a: any, b: any) => b.startMonth - a.startMonth
    );

    params.push(
      new YearDataModel(
        item.year,
        sortedMinWages,
        item.minWageEmployeeTaxExemption,
        taxSlices.sort((a: any, b: any) => a.rate - b.rate),
        disabledBases
      )
    );
  }

  return params.sort((a, b) => b.year - a.year);
}

// ============================================================================
// Validation
// ============================================================================

function validateInput(input: unknown): { valid: true; data: CalculationInput } | { valid: false; errors: string[] } {
  const errors: string[] = [];

  if (typeof input !== 'object' || input === null) {
    return { valid: false, errors: ['Input must be a JSON object'] };
  }

  const obj = input as Record<string, unknown>;

  // Required fields with types
  const requiredFields: { name: keyof CalculationInput; type: string; validate?: (v: unknown) => boolean }[] = [
    { name: 'year', type: 'number', validate: v => Number.isInteger(v) && (v as number) >= 2016 && (v as number) <= 2030 },
    { name: 'calculationMode', type: 'string', validate: v => ['GROSS_TO_NET', 'NET_TO_GROSS', 'TOTAL_TO_GROSS'].includes(v as string) },
    { name: 'employeeTypeId', type: 'number', validate: v => Number.isInteger(v) && (v as number) >= 1 },
    { name: 'salaryAmount', type: 'number', validate: v => typeof v === 'number' && v >= 0 },
    { name: 'workedDays', type: 'number', validate: v => Number.isInteger(v) && (v as number) >= 1 && (v as number) <= 30 },
    { name: 'researchAndDevelopmentWorkedDays', type: 'number', validate: v => Number.isInteger(v) && (v as number) >= 0 && (v as number) <= 30 },
    { name: 'disabilityDegree', type: 'number', validate: v => Number.isInteger(v) && (v as number) >= 0 && (v as number) <= 3 },
    { name: 'agiRate', type: 'number', validate: v => typeof v === 'number' && v >= 0 && v <= 1 },
    { name: 'educationExemptionRate', type: 'number', validate: v => typeof v === 'number' && v >= 0 && v <= 1 },
    { name: 'isPensioner', type: 'boolean' },
    { name: 'applyEmployerDiscount5746', type: 'boolean' },
    { name: 'isAgiIncludedNet', type: 'boolean' },
    { name: 'isAgiIncludedTax', type: 'boolean' },
    { name: 'applyMinWageTaxExemption', type: 'boolean' },
    { name: 'isAgiCalculationEnabled', type: 'boolean' },
  ];

  for (const field of requiredFields) {
    if (!(field.name in obj)) {
      errors.push(`Missing required field: ${field.name}`);
      continue;
    }

    const value = obj[field.name];
    if (typeof value !== field.type) {
      errors.push(`Field '${field.name}' must be of type ${field.type}, got ${typeof value}`);
      continue;
    }

    if (field.validate && !field.validate(value)) {
      errors.push(`Field '${field.name}' has invalid value: ${JSON.stringify(value)}`);
    }
  }

  // Cross-field validation
  if (!errors.length) {
    const data = obj as unknown as CalculationInput;

    // Validate year exists in parameters
    if (!YEAR_PARAMETERS.find((y: any) => y.year === data.year)) {
      errors.push(`Year ${data.year} not found in year parameters. Available: ${YEAR_PARAMETERS.map((y: any) => y.year).join(', ')}`);
    }

    // Validate employee type exists
    if (!EMPLOYEE_TYPES.find((t: any) => t.id === data.employeeTypeId)) {
      errors.push(`Employee type ${data.employeeTypeId} not found. Available: ${EMPLOYEE_TYPES.map((t: any) => t.id).join(', ')}`);
    }

    // R&D days cannot exceed worked days
    if (data.researchAndDevelopmentWorkedDays > data.workedDays) {
      errors.push(`R&D days (${data.researchAndDevelopmentWorkedDays}) cannot exceed worked days (${data.workedDays})`);
    }
  }

  if (errors.length > 0) {
    return { valid: false, errors };
  }

  return { valid: true, data: obj as unknown as CalculationInput };
}

// ============================================================================
// Calculation
// ============================================================================

function extractMonthOutput(month: any): MonthOutput {
  return {
    monthNumber: month.monthNumber,
    calculatedGrossSalary: month.calculatedGrossSalary,
    sgkBase: month.SGKBase,
    incomeTaxBase: month.incomeTaxBase,
    cumulativeIncomeTaxBase: month.cumulativeIncomeTaxBase,
    employeeSgkDeduction: month.employeeSGKDeduction,
    employeeSgkExemption: month.employeeSGKExemption,
    employeeFinalSgkDeduction: month.employeeFinalSGKDeduction,
    employeeUnemploymentInsuranceDeduction: month.employeeUnemploymentInsuranceDeduction,
    employeeUnemploymentInsuranceExemption: month.employeeUnemploymentInsuranceExemption,
    employeeFinalUnemploymentInsuranceDeduction: month.employeeFinalUnemploymentInsuranceDeduction,
    employeeIncomeTax: month.employeeIncomeTax,
    employeeIncomeTaxExemption: month.employeeIncomeTaxExemptionAmount,
    stampTax: month.stampTax,
    employeeStampTaxExemption: month.employeeStampTaxExemption,
    employerStampTaxExemption: month.employerStampTaxExemption,
    totalStampTaxExemption: month.totalStampTaxExemption,
    employerStampTax: month.employerStampTax,
    netSalary: month.netSalary,
    agiAmount: month.AGIAmount,
    finalNetSalary: month.finalNetSalary,
    employerSgkDeduction: month.employerSGKDeduction,
    employerSgkExemption: month.employerSGKExemption,
    employerFinalSgkDeduction: month.employerFinalSGKDeduction,
    employerUnemploymentInsuranceDeduction: month.employerUnemploymentInsuranceDeduction,
    employerUnemploymentInsuranceExemption: month.employerUnemploymentInsuranceExemption,
    employerFinalUnemploymentInsuranceDeduction: month.employerFinalUnemploymentInsuranceDeduction,
    employerIncomeTaxExemption: month.employerIncomeTaxExemptionAmount,
    employerFinalIncomeTax: month.employerFinalIncomeTax,
    employerTotalSgkCost: month.employerTotalSGKCost,
    employerTotalCost: month.employerTotalCost,
    totalSgkExemption: month.totalSGKExemption,
    employeeMinWageTaxExemption: month.employeeMinWageTaxExemptionAmount,
  };
}

function runCalculation(input: CalculationInput): CalculationOutput {
  const yearParam = YEAR_PARAMETERS.find((y: any) => y.year === input.year);
  const employeeType = EMPLOYEE_TYPES.find((t: any) => t.id === input.employeeTypeId);
  const standardEmployeeType = EMPLOYEE_TYPES.find((t: any) => t.id === 1)!;

  // Find options by rates/degrees
  const agiOption = AGI_OPTIONS.find((a: any) => a.rate === input.agiRate) || AGI_OPTIONS[0];
  const eduType = EMPLOYEE_EDUCATION_TYPES.find((e: any) => e.exemptionRate === input.educationExemptionRate)
    || EMPLOYEE_EDUCATION_TYPES[0];
  const disabilityOption = DISABILITY_OPTIONS.find((d: any) => d.degree === input.disabilityDegree)
    || DISABILITY_OPTIONS[0];

  // Create YearCalculationModel - EXACT same logic Angular UI uses
  const yearCalc = new YearCalculationModel(MONTHS, CALCULATION_CONSTANTS, standardEmployeeType);
  yearCalc.year = yearParam;
  yearCalc.calculationMode = input.calculationMode;
  yearCalc.employeeType = employeeType;
  yearCalc.employeeEduType = eduType;
  yearCalc.employeeDisability = disabilityOption;
  yearCalc.AGI = agiOption;
  yearCalc.isPensioner = input.isPensioner;
  yearCalc.employerDiscount5746 = input.applyEmployerDiscount5746;
  yearCalc.isAGIIncludedNet = input.isAgiIncludedNet;
  yearCalc.isAGIIncludedTax = input.isAgiIncludedTax;
  yearCalc.isAGICalculationEnabled = input.isAgiCalculationEnabled;
  yearCalc.applyMinWageTaxExemption = input.applyMinWageTaxExemption;

  // Set 12 months of uniform data
  yearCalc.enteredAmounts = Array(12).fill(input.salaryAmount);
  yearCalc.dayCounts = Array(12).fill(input.workedDays);
  yearCalc.researchAndDevelopmentWorkedDays = Array(12).fill(input.researchAndDevelopmentWorkedDays);

  // Run calculation
  yearCalc.calculate();

  // Extract results
  const monthOutputs: MonthOutput[] = yearCalc.months.map(extractMonthOutput);

  return {
    months: monthOutputs,
    totals: {
      calculatedGrossSalary: yearCalc.calculatedGrossSalary,
      netSalary: yearCalc.netSalary,
      finalNetSalary: yearCalc.finalNetSalary,
      employerTotalCost: yearCalc.employerTotalCost,
      totalSgkExemption: yearCalc.totalSGKExemption,
    },
  };
}

// ============================================================================
// CLI Entry Point
// ============================================================================

async function readStdin(): Promise<string> {
  return new Promise((resolve, reject) => {
    let data = '';
    const rl = readline.createInterface({
      input: process.stdin,
      terminal: false,
    });

    rl.on('line', (line) => {
      data += line;
    });

    rl.on('close', () => {
      resolve(data);
    });

    rl.on('error', (err) => {
      reject(err);
    });

    // Timeout after 5 seconds if no input
    setTimeout(() => {
      if (!data) {
        rl.close();
        reject(new Error('Timeout: no input received within 5 seconds'));
      }
    }, 5000);
  });
}

function writeResponse(response: CliResponse): void {
  console.log(JSON.stringify(response));
}

function writeError(code: ErrorResponse['error']['code'], message: string, details?: string[]): never {
  const response: ErrorResponse = {
    success: false,
    error: { code, message, details },
  };
  writeResponse(response);

  const exitCode = code === 'VALIDATION_ERROR' ? 1 : code === 'CALCULATION_ERROR' ? 2 : 3;
  process.exit(exitCode);
}

async function main(): Promise<void> {
  try {
    // Read JSON from stdin
    const inputJson = await readStdin();

    if (!inputJson.trim()) {
      writeError('VALIDATION_ERROR', 'No input provided. Send JSON via stdin.');
    }

    // Parse JSON
    let parsed: unknown;
    try {
      parsed = JSON.parse(inputJson);
    } catch (e) {
      writeError('VALIDATION_ERROR', 'Invalid JSON', [(e as Error).message]);
    }

    // Validate input
    const validation = validateInput(parsed);
    if (!validation.valid) {
      writeError('VALIDATION_ERROR', 'Input validation failed', validation.errors);
    }

    // Run calculation
    const output = runCalculation(validation.data);

    // Write success response
    const response: SuccessResponse = {
      success: true,
      input: validation.data,
      output,
      calculatedAt: new Date().toISOString(),
    };
    writeResponse(response);
    process.exit(0);

  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    const stack = error instanceof Error ? error.stack : undefined;
    writeError('INTERNAL_ERROR', message, stack ? [stack] : undefined);
  }
}

main();
