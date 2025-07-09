public class NarrationSteps
{
    private readonly List<Action> _steps;
    private int _currentStep;

    // Constructor for List<Action>
    public NarrationSteps(List<Action> steps)
    {
        _steps = steps;
        _currentStep = 0;
    }

    // Constructor for List<string>
    public NarrationSteps(List<string> narrationSteps, Action<string> updateNarration)
    {
        _steps = new List<Action>();
        foreach (var narration in narrationSteps)
        {
            _steps.Add(() => updateNarration(narration));
        }
        _currentStep = 0;
    }

    public void Start()
    {
        _currentStep = 0;
        if (_steps.Count > 0)
            _steps[0]();
    }

    public void Next()
    {
        if (_currentStep < _steps.Count - 1)
        {
            _currentStep++;
            _steps[_currentStep]();
        }
    }

    public void Previous()
    {
        if (_currentStep > 0)
        {
            _currentStep--;
            _steps[_currentStep]();
        }
    }

    public int CurrentStep => _currentStep;
    public int StepCount => _steps.Count;
}
