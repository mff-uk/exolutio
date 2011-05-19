using System;
using EvoX.Controller.Commands.Reflection;

namespace EvoX.View.Commands
{
    public interface IOperationParameterControl<TParameterType>
    {
        TParameterType Value { get; }
        void SetSuggestedValue(object suggestedValue);
    }

    public interface IOperationParameterControl
    {
        object Value { get; }

        void InitControl();

        void SetSuggestedValue(object suggestedValue);
    }

    public interface IOperationParameterControlWithConsistencyCheck
    {
        void LoadAllPossibleValues(Type componentType);
        void MakeValuesConsistentWith(object superiorObject);
        IOperationParameterControl SuperiorPropertyEditor { get; set; }
        ParameterConsistency ConsistencyChecker { get; set; }
        void OnSuperiorPropertyChanged(object sender, EventArgs e);
    }
}