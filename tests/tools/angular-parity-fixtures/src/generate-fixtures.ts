/**
 * Angular Parity Fixture Generator
 *
 * This script imports the Angular salary calculator models DIRECTLY and generates
 * JSON fixtures for verifying the .NET implementation.
 *
 * Usage: npm run generate
 */

import * as fs from 'fs';
import * as path from 'path';

// Types extracted from Angular (no Angular runtime deps)
import type {
  CalculationConstants,
  EmployeeType,
  EmployeeEducationType,
  DisabilityOption,
  AGIOption,
} from './angular-types';

// ============================================================================
// Direct imports from Angular project - NO COPYING OF LOGIC
// Angular models are pure TypeScript with no framework dependencies
// ============================================================================

// Use require for Angular models to avoid rootDir issues
const angularModelsPath = path.resolve(__dirname, '../../../../external/maas-hesaplama/src/app/core/models');

// We'll dynamically load the models after ts-node registers them
const { MonthCalculationModel } = require(path.join(angularModelsPath, 'month-calculation.model'));
const { YearCalculationModel } = require(path.join(angularModelsPath, 'year-calculation.model'));
const {
  YearDataModel,
  TaxSliceModel,
  MinGrossWage,
  DisabledMonthlyIncomeTaxDiscountBaseModel
} = require(path.join(angularModelsPath, 'year-data.model'));

// ============================================================================
// Load Angular project's data files directly (JSON)
// ============================================================================

const angularBasePath = path.resolve(__dirname, '../../../../external/maas-hesaplama/src');
const fixturesJson = require(path.join(angularBasePath, 'assets/fixtures.json'));
const yearParametersJson = require(path.join(angularBasePath, 'assets/year-parameters.json'));

// Extract constants and options from fixtures.json (same structure Angular uses)
const MONTHS: string[] = fixturesJson.MONTHS;
const CALCULATION_CONSTANTS: CalculationConstants = fixturesJson.CALCULATION_CONSTANTS;
const EMPLOYEE_TYPES: EmployeeType[] = fixturesJson.EMPLOYEE_TYPES.options;
const EMPLOYEE_EDUCATION_TYPES: EmployeeEducationType[] = fixturesJson.EMPLOYEE_EDUCATION_TYPES.options;
const DISABILITY_OPTIONS: DisabilityOption[] = fixturesJson.DISABILITY_OPTIONS.options;
const AGI_OPTIONS: AGIOption[] = fixturesJson.AGI_OPTIONS.options;

// Parse year parameters into YearDataModel instances (same as ParametersService does)
function parseYearParameters(): any[] {
  const params: any[] = [];

  for (const item of yearParametersJson.yearParameters) {
    const taxSlices = item.taxSlices.map(
      (ts: { rate: number; ceil: number }) => new TaxSliceModel(ts.rate, ts.ceil)
    );

    const disabledBases =
      item.disabledMonthlyIncomeTaxDiscountBases.map(
        (d: { degree: number; amount: number }) =>
          new DisabledMonthlyIncomeTaxDiscountBaseModel(d.degree, d.amount)
      );

    // Sort minGrossWages by startMonth descending (same as Angular)
    const sortedMinWages = [...item.minGrossWages].sort(
      (a: any, b: any) => b.startMonth - a.startMonth
    );

    params.push(
      new YearDataModel(
        item.year,
        sortedMinWages,
        item.minWageEmployeeTaxExemption,
        taxSlices.sort((a: any, b: any) => a.rate - b.rate), // Sort ascending by rate
        disabledBases
      )
    );
  }

  // Sort by year descending
  return params.sort((a, b) => b.year - a.year);
}

const YEAR_PARAMETERS = parseYearParameters();

// ============================================================================
// Fixture Types (for JSON serialization)
// ============================================================================

interface TestFixture {
  testId: string;
  description: string;
  input: TestInput;
  expectedOutput: TestOutput;
  generatedAt: string;
  angularVersion: string;
}

interface TestInput {
  year: number;
  calculationMode: string;
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

interface TestOutput {
  months: MonthOutput[];
  totals: TotalsOutput;
}

// ============================================================================
// Test Scenario Definition
// ============================================================================

interface TestScenario {
  testId: string;
  description: string;
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

// ============================================================================
// Extract output from Angular models
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

// ============================================================================
// Run calculation using Angular's YearCalculationModel
// ============================================================================

function runCalculation(scenario: TestScenario): TestFixture {
  const yearParam = YEAR_PARAMETERS.find((y: any) => y.year === scenario.year);
  if (!yearParam) {
    throw new Error(`Year ${scenario.year} not found in year parameters`);
  }

  const employeeType = EMPLOYEE_TYPES.find(t => t.id === scenario.employeeTypeId);
  if (!employeeType) {
    throw new Error(`Employee type ${scenario.employeeTypeId} not found`);
  }

  const standardEmployeeType = EMPLOYEE_TYPES.find(t => t.id === 1)!;

  // Find AGI option by rate
  const agiOption = AGI_OPTIONS.find(a => a.rate === scenario.agiRate) || AGI_OPTIONS[0];

  // Find education type by exemption rate
  const eduType = EMPLOYEE_EDUCATION_TYPES.find(e => e.exemptionRate === scenario.educationExemptionRate)
    || EMPLOYEE_EDUCATION_TYPES[0];

  // Find disability option by degree
  const disabilityOption = DISABILITY_OPTIONS.find(d => d.degree === scenario.disabilityDegree)
    || DISABILITY_OPTIONS[0];

  // Use YearCalculationModel - the EXACT same logic Angular UI uses
  const yearCalc = new YearCalculationModel(MONTHS, CALCULATION_CONSTANTS, standardEmployeeType);
  yearCalc.year = yearParam;
  yearCalc.calculationMode = scenario.calculationMode;
  yearCalc.employeeType = employeeType;
  yearCalc.employeeEduType = eduType;
  yearCalc.employeeDisability = disabilityOption;
  yearCalc.AGI = agiOption;
  yearCalc.isPensioner = scenario.isPensioner;
  yearCalc.employerDiscount5746 = scenario.applyEmployerDiscount5746;
  yearCalc.isAGIIncludedNet = scenario.isAgiIncludedNet;
  yearCalc.isAGIIncludedTax = scenario.isAgiIncludedTax;
  yearCalc.isAGICalculationEnabled = scenario.isAgiCalculationEnabled;
  yearCalc.applyMinWageTaxExemption = scenario.applyMinWageTaxExemption;

  // Set 12 months of salary data
  const amounts = Array(12).fill(scenario.salaryAmount);
  const workedDays = Array(12).fill(scenario.workedDays);
  const rdDays = Array(12).fill(scenario.researchAndDevelopmentWorkedDays);

  yearCalc.enteredAmounts = amounts;
  yearCalc.dayCounts = workedDays;
  yearCalc.researchAndDevelopmentWorkedDays = rdDays;

  // Run calculation
  yearCalc.calculate();

  // Extract month outputs
  const monthOutputs: MonthOutput[] = yearCalc.months.map(extractMonthOutput);

  const fixture: TestFixture = {
    testId: scenario.testId,
    description: scenario.description,
    input: {
      year: scenario.year,
      calculationMode: scenario.calculationMode,
      employeeTypeId: scenario.employeeTypeId,
      salaryAmount: scenario.salaryAmount,
      workedDays: scenario.workedDays,
      researchAndDevelopmentWorkedDays: scenario.researchAndDevelopmentWorkedDays,
      disabilityDegree: scenario.disabilityDegree,
      agiRate: scenario.agiRate,
      educationExemptionRate: scenario.educationExemptionRate,
      isPensioner: scenario.isPensioner,
      applyEmployerDiscount5746: scenario.applyEmployerDiscount5746,
      isAgiIncludedNet: scenario.isAgiIncludedNet,
      isAgiIncludedTax: scenario.isAgiIncludedTax,
      applyMinWageTaxExemption: scenario.applyMinWageTaxExemption,
      isAgiCalculationEnabled: scenario.isAgiCalculationEnabled,
    },
    expectedOutput: {
      months: monthOutputs,
      totals: {
        calculatedGrossSalary: yearCalc.calculatedGrossSalary,
        netSalary: yearCalc.netSalary,
        finalNetSalary: yearCalc.finalNetSalary,
        employerTotalCost: yearCalc.employerTotalCost,
        totalSgkExemption: yearCalc.totalSGKExemption,
      },
    },
    generatedAt: new Date().toISOString(),
    angularVersion: '21.x',
  };

  return fixture;
}

// ============================================================================
// Test Scenarios
// ============================================================================

function generateTestScenarios(): TestScenario[] {
  const scenarios: TestScenario[] = [];

  // Get all supported years
  const years = YEAR_PARAMETERS.map((y: any) => y.year);
  console.log(`Supported years: ${years.join(', ')}`);

  // -------------------------------------------------------------------------
  // Core scenarios covering key combinations
  // -------------------------------------------------------------------------

  // Standard employee scenarios across years
  for (const year of [2026, 2025, 2024, 2023, 2022, 2021]) {
    if (!years.includes(year)) continue;

    scenarios.push({
      testId: `${year}-standard-grosstonet-50000-30days`,
      description: `Year ${year}, Standard Employee, Gross to Net, 50000 TL, 30 days`,
      year, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 50000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    });
  }

  // All calculation modes for 2026
  scenarios.push(
    {
      testId: '2026-standard-netgross-30000-30days',
      description: 'Year 2026, Standard Employee, Net to Gross, 30000 TL, 30 days',
      year: 2026, calculationMode: 'NET_TO_GROSS', employeeTypeId: 1, salaryAmount: 30000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    },
    {
      testId: '2026-standard-totalgross-70000-30days',
      description: 'Year 2026, Standard Employee, Total to Gross, 70000 TL, 30 days',
      year: 2026, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: 1, salaryAmount: 70000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    }
  );

  // Employee types coverage (2026)
  const employeeTypeScenarios = [
    { id: 1, name: 'standard', salary: 50000, rdDays: 0 },
    { id: 2, name: 'teknokent-4691', salary: 60000, rdDays: 30 },
    { id: 3, name: 'arge-5746', salary: 70000, rdDays: 30 },
    { id: 4, name: 'law6111', salary: 50000, rdDays: 0 },
    { id: 5, name: 'employer', salary: 80000, rdDays: 0 },
    { id: 6, name: 'personnel-27103', salary: 45000, rdDays: 0 },
    { id: 7, name: 'personnel-17103', salary: 45000, rdDays: 0 },
    { id: 8, name: 'employer-teknokent', salary: 90000, rdDays: 30 },
    { id: 9, name: 'employer-arge', salary: 90000, rdDays: 30 },
    { id: 10, name: 'domestic', salary: 33030, rdDays: 0 },
  ];

  for (const et of employeeTypeScenarios) {
    scenarios.push({
      testId: `2026-${et.name}-grosstonet-${et.salary}-30days`,
      description: `Year 2026, Employee Type ${et.id} (${et.name}), Gross to Net`,
      year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: et.id, salaryAmount: et.salary,
      workedDays: 30, researchAndDevelopmentWorkedDays: et.rdDays, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: et.id === 3,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    });
  }

  // Disability degrees (2026)
  for (const degree of [1, 2, 3]) {
    scenarios.push({
      testId: `2026-standard-disabled${degree}-grosstonet-60000`,
      description: `Year 2026, Standard Employee, ${degree}st/nd/rd Degree Disabled`,
      year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 60000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: degree, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    });
  }

  // Pensioner scenario
  scenarios.push({
    testId: '2026-standard-pensioner-grosstonet-50000',
    description: 'Year 2026, Standard Employee, Pensioner, Gross to Net',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 50000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: true, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // Partial month scenario
  scenarios.push({
    testId: '2026-standard-grosstonet-50000-15days',
    description: 'Year 2026, Standard Employee, 15 worked days',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 50000,
    workedDays: 15, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // AGI enabled scenario (for pre-2022 years)
  scenarios.push({
    testId: '2021-standard-grosstonet-10000-agi-enabled',
    description: 'Year 2021, Standard Employee, AGI Enabled',
    year: 2021, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 10000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: true,
  });

  // R&D partial days scenario
  scenarios.push({
    testId: '2026-teknokent-grosstonet-60000-20rd',
    description: 'Year 2026, Teknokent, 30 days worked, 20 R&D days',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 2, salaryAmount: 60000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 20, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: true,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // High salary to test tax brackets
  scenarios.push({
    testId: '2026-standard-grosstonet-200000-30days',
    description: 'Year 2026, Standard Employee, High salary (200k)',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 200000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // Minimum wage scenario
  scenarios.push({
    testId: '2026-standard-minwage-grosstonet',
    description: 'Year 2026, Standard Employee, Minimum Wage',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 33030,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // -------------------------------------------------------------------------
  // EXTENDED EDGE CASES
  // -------------------------------------------------------------------------

  // ===== 1. MID-YEAR WAGE CHANGE YEARS (2022, 2023) - More employee types =====
  // These years have Jan->July minimum wage transitions - critical for exemption calculations

  // 2022 - More employee types
  const midYearEmployeeTypes = [
    { id: 2, name: 'teknokent-4691', salary: 40000, rdDays: 30 },
    { id: 3, name: 'arge-5746', salary: 45000, rdDays: 30 },
    { id: 5, name: 'employer', salary: 50000, rdDays: 0 },
    { id: 6, name: 'personnel-27103', salary: 25000, rdDays: 0 },
  ];

  for (const et of midYearEmployeeTypes) {
    scenarios.push({
      testId: `2022-${et.name}-grosstonet-${et.salary}-30days`,
      description: `Year 2022, Employee Type ${et.id} (${et.name}), mid-year wage change test`,
      year: 2022, calculationMode: 'GROSS_TO_NET', employeeTypeId: et.id, salaryAmount: et.salary,
      workedDays: 30, researchAndDevelopmentWorkedDays: et.rdDays, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: et.id === 3,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    });
  }

  // 2023 - More employee types (larger wage jump mid-year)
  for (const et of midYearEmployeeTypes) {
    const salary23 = Math.round(et.salary * 1.5); // 2023 had higher wages
    scenarios.push({
      testId: `2023-${et.name}-grosstonet-${salary23}-30days`,
      description: `Year 2023, Employee Type ${et.id} (${et.name}), mid-year wage change test`,
      year: 2023, calculationMode: 'GROSS_TO_NET', employeeTypeId: et.id, salaryAmount: salary23,
      workedDays: 30, researchAndDevelopmentWorkedDays: et.rdDays, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: et.id === 3,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    });
  }

  // ===== 2. MORE REVERSE CALCULATION MODES (NetToGross, TotalToGross) =====

  // NetToGross for different employee types
  const reverseCalcTypes = [
    { id: 2, name: 'teknokent', netAmount: 35000, rdDays: 30 },
    { id: 3, name: 'arge-5746', netAmount: 40000, rdDays: 30 },
    { id: 5, name: 'employer', netAmount: 50000, rdDays: 0 },
    { id: 6, name: 'personnel-27103', netAmount: 25000, rdDays: 0 },
  ];

  for (const et of reverseCalcTypes) {
    scenarios.push({
      testId: `2026-${et.name}-netgross-${et.netAmount}-30days`,
      description: `Year 2026, ${et.name}, Net to Gross, ${et.netAmount} TL`,
      year: 2026, calculationMode: 'NET_TO_GROSS', employeeTypeId: et.id, salaryAmount: et.netAmount,
      workedDays: 30, researchAndDevelopmentWorkedDays: et.rdDays, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: et.id === 3,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    });
  }

  // TotalToGross for different employee types
  // Angular binary search bug fixed in v4.0.1 - all employee types now supported
  for (const et of reverseCalcTypes) {
    const totalAmount = Math.round(et.netAmount * 1.6); // Rough employer cost
    scenarios.push({
      testId: `2026-${et.name}-totalgross-${totalAmount}-30days`,
      description: `Year 2026, ${et.name}, Total to Gross, ${totalAmount} TL`,
      year: 2026, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: et.id, salaryAmount: totalAmount,
      workedDays: 30, researchAndDevelopmentWorkedDays: et.rdDays, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: et.id === 3,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    });
  }

  // Reverse calculations for mid-year wage change years
  scenarios.push(
    {
      testId: '2022-standard-netgross-25000-30days',
      description: 'Year 2022, Net to Gross, mid-year wage test',
      year: 2022, calculationMode: 'NET_TO_GROSS', employeeTypeId: 1, salaryAmount: 25000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    },
    {
      testId: '2023-standard-netgross-40000-30days',
      description: 'Year 2023, Net to Gross, mid-year wage test',
      year: 2023, calculationMode: 'NET_TO_GROSS', employeeTypeId: 1, salaryAmount: 40000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    },
    {
      testId: '2022-standard-totalgross-50000-30days',
      description: 'Year 2022, Total to Gross, mid-year wage test',
      year: 2022, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: 1, salaryAmount: 50000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    }
  );

  // ===== 3. SGK CEILING EDGE CASES =====
  // 2026 SGK ceiling: 264,052.80 TRY (8 × 33,006.60)

  // At exactly SGK ceiling
  scenarios.push({
    testId: '2026-standard-grosstonet-264053-sgkceiling',
    description: 'Year 2026, Salary at SGK ceiling (264,053 TL)',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 264053,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // Above SGK ceiling - SGK should cap
  scenarios.push({
    testId: '2026-standard-grosstonet-350000-aboveceiling',
    description: 'Year 2026, Salary above SGK ceiling (350,000 TL)',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 350000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // Very high salary - tests highest tax bracket (40%)
  scenarios.push({
    testId: '2026-standard-grosstonet-500000-veryhigh',
    description: 'Year 2026, Very high salary (500,000 TL) - 40% bracket',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 500000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // R&D employee at ceiling - tests exemption calculations at limits
  scenarios.push({
    testId: '2026-teknokent-grosstonet-300000-highrd',
    description: 'Year 2026, Teknokent, High salary near ceiling',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 2, salaryAmount: 300000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 30, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: true,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // ===== 4. R&D DAYS VARIATIONS =====

  // R&D = 10 days (1/3 of worked days)
  scenarios.push({
    testId: '2026-teknokent-grosstonet-70000-10rd',
    description: 'Year 2026, Teknokent, 30 days worked, 10 R&D days (1/3)',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 2, salaryAmount: 70000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 10, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: true,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // R&D = 15 days (half of worked days)
  scenarios.push({
    testId: '2026-arge5746-grosstonet-80000-15rd',
    description: 'Year 2026, AR-GE 5746, 30 days worked, 15 R&D days (half)',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 3, salaryAmount: 80000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 15, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: true,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // R&D = 5 days (minimal R&D)
  scenarios.push({
    testId: '2026-teknokent-grosstonet-60000-5rd',
    description: 'Year 2026, Teknokent, 30 days worked, 5 R&D days (minimal)',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 2, salaryAmount: 60000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 5, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: true,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // Partial month with R&D (15 worked, 10 R&D)
  scenarios.push({
    testId: '2026-teknokent-grosstonet-60000-15days-10rd',
    description: 'Year 2026, Teknokent, 15 worked days, 10 R&D days',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 2, salaryAmount: 60000,
    workedDays: 15, researchAndDevelopmentWorkedDays: 10, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: true,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // ===== 5. PARTIAL MONTH VARIATIONS =====

  // 1 day worked (edge case)
  scenarios.push({
    testId: '2026-standard-grosstonet-50000-1day',
    description: 'Year 2026, Standard Employee, 1 worked day (edge case)',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 50000,
    workedDays: 1, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // 20 days worked
  scenarios.push({
    testId: '2026-standard-grosstonet-50000-20days',
    description: 'Year 2026, Standard Employee, 20 worked days',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 50000,
    workedDays: 20, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // ===== 6. COMBINED EDGE CASES =====

  // Disabled + High salary
  scenarios.push({
    testId: '2026-standard-disabled1-grosstonet-200000',
    description: 'Year 2026, 1st Degree Disabled, High salary (200k)',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 200000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: 1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // Pensioner + R&D type
  scenarios.push({
    testId: '2026-teknokent-pensioner-grosstonet-70000',
    description: 'Year 2026, Teknokent Pensioner',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 2, salaryAmount: 70000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 30, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: true, applyEmployerDiscount5746: true,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // Disabled + Partial month
  scenarios.push({
    testId: '2026-standard-disabled2-grosstonet-60000-15days',
    description: 'Year 2026, 2nd Degree Disabled, 15 days',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 60000,
    workedDays: 15, researchAndDevelopmentWorkedDays: 0, disabilityDegree: 2, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // R&D + Disabled
  scenarios.push({
    testId: '2026-arge5746-disabled3-grosstonet-80000',
    description: 'Year 2026, AR-GE 5746, 3rd Degree Disabled',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 3, salaryAmount: 80000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 30, disabilityDegree: 3, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: true,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // ===== 7. AGI VARIATIONS (Pre-2022) =====

  // Different AGI rates for 2021
  scenarios.push(
    {
      testId: '2021-standard-grosstonet-10000-agi-single',
      description: 'Year 2021, AGI enabled, Single (50%)',
      year: 2021, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 10000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: true,
    },
    {
      testId: '2021-standard-grosstonet-10000-agi-married-nonworking',
      description: 'Year 2021, AGI enabled, Married non-working spouse (60%)',
      year: 2021, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 10000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.6,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: true,
    },
    {
      testId: '2021-standard-grosstonet-10000-agi-married-1child',
      description: 'Year 2021, AGI enabled, Married + 1 child (67.5%)',
      year: 2021, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 10000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.675,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: true,
    }
  );

  // 2021 R&D with AGI
  scenarios.push({
    testId: '2021-teknokent-grosstonet-15000-agi',
    description: 'Year 2021, Teknokent with AGI enabled',
    year: 2021, calculationMode: 'GROSS_TO_NET', employeeTypeId: 2, salaryAmount: 15000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 30, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: true,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: true,
  });

  // ===== 8. EDUCATION EXEMPTION RATES (for 5746) =====
  // Legal rates defined by Law 5746: 0.80, 0.90, 0.95 (no 100% option exists)

  // Different education rates for 5746 employees
  scenarios.push(
    {
      testId: '2026-arge5746-grosstonet-80000-edu95',
      description: 'Year 2026, AR-GE 5746, 95% education exemption (PhD/Masters in Fundamental Sciences)',
      year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 3, salaryAmount: 80000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 30, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.95, isPensioner: false, applyEmployerDiscount5746: true,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    },
    {
      testId: '2026-arge5746-grosstonet-80000-edu80',
      description: 'Year 2026, AR-GE 5746, 80% education exemption (Other R&D Personnel)',
      year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 3, salaryAmount: 80000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 30, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.8, isPensioner: false, applyEmployerDiscount5746: true,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    }
  );

  // ===== 9. MIN WAGE TAX EXEMPTION TOGGLE =====

  // Without min wage tax exemption (pre-2022 behavior simulation)
  scenarios.push({
    testId: '2026-standard-grosstonet-50000-no-minwage-exemption',
    description: 'Year 2026, Standard, WITHOUT min wage tax exemption',
    year: 2026, calculationMode: 'GROSS_TO_NET', employeeTypeId: 1, salaryAmount: 50000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: false,
    isAgiCalculationEnabled: false,
  });

  // ===== 10. COMPREHENSIVE TOTAL_TO_GROSS TEST SUITE =====
  // Testing binary search convergence across all employee types and scenarios
  // Angular binary search bug fixed in v4.0.1 - all employee types now supported

  // All employee types with TOTAL_TO_GROSS for 2026
  const totalToGrossEmployeeTypes = [
    { id: 1, name: 'standard', totalAmount: 80000, rdDays: 0 },
    { id: 2, name: 'teknokent-4691', totalAmount: 80000, rdDays: 30 },
    { id: 3, name: 'arge-5746', totalAmount: 80000, rdDays: 30 },
    { id: 4, name: 'law6111', totalAmount: 80000, rdDays: 0 },
    { id: 5, name: 'employer', totalAmount: 80000, rdDays: 0 },
    { id: 6, name: 'personnel-27103', totalAmount: 60000, rdDays: 0 },
    { id: 7, name: 'personnel-17103', totalAmount: 60000, rdDays: 0 },
    { id: 8, name: 'employer-teknokent', totalAmount: 100000, rdDays: 30 }, // No SGK
    { id: 9, name: 'employer-arge', totalAmount: 100000, rdDays: 30 }, // No SGK
    { id: 10, name: 'domestic', totalAmount: 50000, rdDays: 0 },
  ];

  for (const et of totalToGrossEmployeeTypes) {
    // Skip if already covered by earlier scenarios
    const existingIds = [
      '2026-standard-totalgross-70000-30days',
      '2026-teknokent-totalgross-56000-30days',
      '2026-arge-5746-totalgross-64000-30days',
      '2026-personnel-27103-totalgross-40000-30days',
      '2026-employer-totalgross-80000-30days',
    ];
    const testId = `2026-${et.name}-totalgross-${et.totalAmount}-30days`;
    if (existingIds.includes(testId)) continue;

    scenarios.push({
      testId,
      description: `Year 2026, ${et.name} (ID=${et.id}), Total to Gross, ${et.totalAmount} TL`,
      year: 2026, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: et.id, salaryAmount: et.totalAmount,
      workedDays: 30, researchAndDevelopmentWorkedDays: et.rdDays, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: et.id === 3,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    });
  }

  // NOTE: TotalToGross with partial months is semantically complex - Angular's binary search
  // operates at 30 days then recalculates at partial days, which is a design choice.
  // These scenarios are omitted as they test implementation details rather than domain logic.

  // TotalToGross at SGK ceiling boundary (tests edge of SGK calculations)
  scenarios.push({
    testId: '2026-standard-totalgross-350000-sgkceiling',
    description: 'Year 2026, Standard, Total to Gross near SGK ceiling',
    year: 2026, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: 1, salaryAmount: 350000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // TotalToGross at minimum wage (tests floor calculations)
  scenarios.push({
    testId: '2026-standard-totalgross-40000-minwage',
    description: 'Year 2026, Standard, Total to Gross near minimum wage',
    year: 2026, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: 1, salaryAmount: 40000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // TotalToGross for years with different min wage periods (2022, 2023)
  // Angular binary search bug fixed in v4.0.1 - all employee types now supported
  scenarios.push(
    {
      testId: '2022-teknokent-totalgross-60000-30days',
      description: 'Year 2022, Teknokent, Total to Gross (mid-year wage change)',
      year: 2022, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: 2, salaryAmount: 60000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 30, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: true,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    },
    {
      testId: '2022-employer-totalgross-50000-30days',
      description: 'Year 2022, Employer, Total to Gross (mid-year wage change)',
      year: 2022, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: 5, salaryAmount: 50000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    },
    {
      testId: '2023-standard-totalgross-80000-30days',
      description: 'Year 2023, Standard, Total to Gross (larger mid-year wage jump)',
      year: 2023, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: 1, salaryAmount: 80000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    },
    {
      testId: '2023-employer-totalgross-75000-30days',
      description: 'Year 2023, Employer, Total to Gross (larger mid-year wage jump)',
      year: 2023, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: 5, salaryAmount: 75000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    }
  );

  // TotalToGross with disability (affects income tax calculations)
  scenarios.push({
    testId: '2026-standard-disabled1-totalgross-100000',
    description: 'Year 2026, Standard, 1st Degree Disabled, Total to Gross',
    year: 2026, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: 1, salaryAmount: 100000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: 1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // TotalToGross for pensioner (different SGK rates)
  scenarios.push({
    testId: '2026-standard-pensioner-totalgross-80000',
    description: 'Year 2026, Standard Pensioner, Total to Gross',
    year: 2026, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: 1, salaryAmount: 80000,
    workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
    educationExemptionRate: 0.9, isPensioner: true, applyEmployerDiscount5746: false,
    isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
    isAgiCalculationEnabled: false,
  });

  // 2021 TotalToGross (minWageEmployeeTaxExemption: false)
  scenarios.push(
    {
      testId: '2021-standard-totalgross-30000-30days',
      description: 'Year 2021, Standard, Total to Gross (pre-exemption era)',
      year: 2021, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: 1, salaryAmount: 30000,
      workedDays: 30, researchAndDevelopmentWorkedDays: 0, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: false,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: true,
    }
  );

  // ===== 11. SCREENSHOT VERIFICATION TESTS (TotalToGross @ 50k) =====
  // Angular binary search bug fixed in v4.0.1 - all employee types now supported

  const screenshotVerificationTests = [
    { id: 1, name: 'standard', rdDays: 0, applyDiscount: false },
    { id: 2, name: 'teknokent-4691', rdDays: 30, applyDiscount: false },
    { id: 3, name: 'arge-5746', rdDays: 30, applyDiscount: false },
    { id: 4, name: 'law6111', rdDays: 0, applyDiscount: false },
    { id: 5, name: 'employer', rdDays: 0, applyDiscount: false },
    { id: 6, name: 'personnel-27103', rdDays: 0, applyDiscount: false },
    { id: 7, name: 'personnel-17103', rdDays: 0, applyDiscount: false },
    { id: 8, name: 'employer-teknokent', rdDays: 30, applyDiscount: false },
    { id: 9, name: 'employer-arge', rdDays: 30, applyDiscount: false },
    { id: 10, name: 'domestic', rdDays: 0, applyDiscount: false },
  ];

  for (const et of screenshotVerificationTests) {
    scenarios.push({
      testId: `2026-${et.name}-totalgross-50000-screenshot-test`,
      description: `Year 2026, ${et.name} (ID=${et.id}), TotalToGross @ 50k - Screenshot verification`,
      year: 2026, calculationMode: 'TOTAL_TO_GROSS', employeeTypeId: et.id, salaryAmount: 50000,
      workedDays: 30, researchAndDevelopmentWorkedDays: et.rdDays, disabilityDegree: -1, agiRate: 0.5,
      educationExemptionRate: 0.9, isPensioner: false, applyEmployerDiscount5746: et.applyDiscount,
      isAgiIncludedNet: false, isAgiIncludedTax: false, applyMinWageTaxExemption: true,
      isAgiCalculationEnabled: false,
    });
  }

  return scenarios;
}

// ============================================================================
// Main
// ============================================================================

function main() {
  console.log('='.repeat(60));
  console.log('Angular Parity Fixture Generator');
  console.log('Using Angular models DIRECTLY - no copied logic');
  console.log('='.repeat(60));
  console.log();

  const scenarios = generateTestScenarios();
  console.log(`\nGenerating ${scenarios.length} test fixtures...\n`);

  const fixturesDir = path.resolve(__dirname, '../../../artifacts/parity-fixtures');
  if (!fs.existsSync(fixturesDir)) {
    fs.mkdirSync(fixturesDir, { recursive: true });
  }

  // Clean old fixtures
  const existingFiles = fs.readdirSync(fixturesDir).filter(f => f.endsWith('.json'));
  for (const file of existingFiles) {
    fs.unlinkSync(path.join(fixturesDir, file));
  }

  const results: { testId: string; success: boolean; error?: string }[] = [];

  for (const scenario of scenarios) {
    try {
      const fixture = runCalculation(scenario);
      const fileName = `${fixture.testId}.json`;
      const filePath = path.join(fixturesDir, fileName);
      fs.writeFileSync(filePath, JSON.stringify(fixture, null, 2));
      console.log(`  ✓ ${fixture.testId}`);
      results.push({ testId: scenario.testId, success: true });
    } catch (error: any) {
      console.log(`  ✗ ${scenario.testId}: ${error.message}`);
      results.push({ testId: scenario.testId, success: false, error: error.message });
    }
  }

  // Write summary
  const successCount = results.filter(r => r.success).length;
  const failCount = results.filter(r => !r.success).length;

  console.log('\n' + '='.repeat(60));
  console.log(`Summary: ${successCount} passed, ${failCount} failed`);
  console.log('='.repeat(60));

  // Write manifest
  const manifest = {
    generatedAt: new Date().toISOString(),
    totalFixtures: scenarios.length,
    successCount,
    failCount,
    generatorVersion: '2.0.0',
    note: 'Uses Angular models directly - no logic duplication',
    fixtures: results,
  };
  fs.writeFileSync(path.join(fixturesDir, '_manifest.json'), JSON.stringify(manifest, null, 2));

  if (failCount > 0) {
    process.exit(1);
  }
}

main();
