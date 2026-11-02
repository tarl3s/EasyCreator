namespace EasyCreator;

/// <summary>Placeholder until the Arduino simulator panel is merged.</summary>
public sealed partial class LessonView
{
    void BuildRoboticsActions() => _actions.Children.Add(Ui.Label("Simulador em construção.", 13, Ui.TextDim));
    void StopRobotics() { }
}
