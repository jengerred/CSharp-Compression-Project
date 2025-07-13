// The NarrationSteps class manages a list of steps (actions) for narrating or explaining
// the process in the GUI. It lets the user move forward and backward through each step,
// making it easy to present multi-step tutorials, explanations, or animations.
public class NarrationSteps
{
    // Holds all the steps as actions (things to do, like updating the screen).
    private readonly List<Action> _steps;

    // Keeps track of which step the user is currently on.
    private int _currentStep;

    // Constructor: Accepts a list of actions (each action is a step in the narration).
    // Use this when you already have code actions you want to run for each step.
    public NarrationSteps(List<Action> steps)
    {
        _steps = steps;
        _currentStep = 0;
    }

    // Constructor: Accepts a list of strings (narration text) and an update function.
    // Each string is turned into an action that calls updateNarration with that text.
    // Use this when you just want to show different messages at each step.
    public NarrationSteps(List<string> narrationSteps, Action<string> updateNarration)
    {
        _steps = new List<Action>();
        foreach (var narration in narrationSteps)
        {
            // For each narration string, add an action that updates the narration display.
            _steps.Add(() => updateNarration(narration));
        }
        _currentStep = 0;
    }

    // Starts the narration at the first step.
    // Runs the first action if there are any steps.
    public void Start()
    {
        _currentStep = 0;
        if (_steps.Count > 0)
            _steps[0]();
    }

    // Moves to the next step, if there is one, and runs its action.
    public void Next()
    {
        if (_currentStep < _steps.Count - 1)
        {
            _currentStep++;
            _steps[_currentStep]();
        }
    }

    // Moves back to the previous step, if possible, and runs its action.
    public void Previous()
    {
        if (_currentStep > 0)
        {
            _currentStep--;
            _steps[_currentStep]();
        }
    }

    // Returns the index of the current step (starts at 0).
    public int CurrentStep => _currentStep;

    // Returns the total number of steps in the narration.
    public int StepCount => _steps.Count;
}
