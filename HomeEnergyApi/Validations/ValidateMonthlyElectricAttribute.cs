using HomeEnergyApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HomeEnergyApi.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateMonthlyElectricAttribute : ActionFilterAttribute
    {
        private readonly int _maxValue;
        private readonly int _minValue;
        public ValidateMonthlyElectricAttribute(int minValue, int maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }   
        public override void OnActionExecuting(ActionExecutingContext context)
        {

            if (context.ActionArguments.TryGetValue("homeDto", out var homeDtoObj) && homeDtoObj is HomeDto homeDto)
            {
                if (homeDto.MonthlyElectricUsage > _maxValue || homeDto.MonthlyElectricUsage < _minValue)
                {
                    context.Result = new BadRequestObjectResult($"MonthlyElectricUsage must be between {_minValue} and {_maxValue} kwH.");
                }
            }
        }
    }
}