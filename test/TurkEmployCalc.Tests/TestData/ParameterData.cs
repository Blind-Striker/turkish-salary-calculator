namespace TurkHRSolutions.TurkEmployCalc.Tests.TestData;

internal static class ParameterData
{
    public const string ValidJson =
        """
        {
        	"YearParameters": [{
        		"Year": 2023,
        		"MinGrossWages": [{
        				"StartMonth": 7,
        				"Amount": 13414.5,
        				"SgkCeil": 100608.9
        			},
        			{
        				"StartMonth": 1,
        				"Amount": 10008,
        				"SgkCeil": 75060
        			}
        		],
        		"MinWageEmployeeTaxExemption": true,
        		"TaxSlices": [{
        				"Rate": 0.15,
        				"Ceil": 70000
        			},
        			{
        				"Rate": 0.2,
        				"Ceil": 150000
        			},
        			{
        				"Rate": 0.27,
        				"Ceil": 550000
        			},
        			{
        				"Rate": 0.35,
        				"Ceil": 1900000
        			},
        			{
        				"Rate": 0.4,
        				"Ceil": null
        			}
        		],
        		"DisabledMonthlyIncomeTaxDiscountBases": [{
        				"Degree": 1,
        				"Amount": 4400
        			},
        			{
        				"Degree": 2,
        				"Amount": 2600
        			},
        			{
        				"Degree": 3,
        				"Amount": 1100
        			}
        		]
        	}],
        	"MinimumLivingAllowanceParameters": [{
        			"SpouseWorkStatus": "Unmarried",
        			"ChildrenCompareType": "Eq",
        			"NumberOfChildren": 0,
        			"Rate": 0.5
        		},
        		{
        			"SpouseWorkStatus": "Working",
        			"ChildrenCompareType": "Eq",
        			"NumberOfChildren": 0,
        			"Rate": 0.5
        		}
        	],
        	"EmployeeTypeParameters": [{
        			"Code": "SC",
        			"Description": "Standart Çalışan",
        			"SgkApplicable": true,
        			"SgkMinWageBasedExemption": false,
        			"SgkMinWageBased17103Exemption": false,
        			"UnemploymentInsuranceApplicable": true,
        			"AgiApplicable": true,
        			"IncomeTaxApplicable": true,
        			"TaxMinWageBasedExemption": false,
        			"ResearchAndDevelopmentTaxExemption": false,
        			"StampTaxApplicable": true,
        			"EmployerSgkApplicable": true,
        			"EmployerUnemploymentInsuranceApplicable": true,
        			"EmployerIncomeTaxApplicable": true,
        			"EmployerStampTaxApplicable": true,
        			"EmployerEducationIncomeTaxExemption": false,
        			"EmployerSgkShareTotalExemption": false,
        			"EmployerSgkDiscount5746Applicable": true,
        			"Employer5746AdditionalDiscountApplicable": false
        		}
        	],
        	"CalculationConstantParameters": {
        		"MonthDayCount": 30,
        		"StampTaxRate": 0.00759,
        		"Employee": {
        			"SgkDeductionRate": 0.14,
        			"SgdpDeductionRate": 0.075,
        			"UnemploymentInsuranceRate": 0.01,
        			"PensionerUnemploymentInsuranceRate": 0
        		},
        		"Employer": {
        			"SgkDeductionRate": 0.205,
        			"SgdpDeductionRate": 0.245,
        			"EmployerDiscount5746": 0.05,
        			"UnemploymentInsuranceRate": 0.02,
        			"PensionerUnemploymentInsuranceRate": 0,
        			"Sgk5746AdditionalDiscount": 0.5
        		}
        	},
        	"EmployeeEducationTypeParameters": [{
        			"Code": "DARP",
        			"Description": "Diğer AR-GE Personeli",
        			"ExemptionRate": 0.8
        		}
        	],
        	"DisabilityTypeParameters": [{
        			"Code": "ED0",
        			"Description": "Engelli Değil",
        			"Degree": -1
        		}
        	]
        }
        """;
}
