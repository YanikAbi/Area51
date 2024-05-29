public enum SecurityLevel
{
    Confidential,
    Secret,
    TopSecret
}

public enum FloorLevel
{
    G,
    S,
    T1,
    T2
}


public class Agent
{
    public SecurityLevel SecurityLevel { get; private set; }
    public FloorLevel CurrentFloor { get; set; }
    private Elevator _elevator;

    public Agent(SecurityLevel securityLevel, Elevator elevator)
    {
        SecurityLevel = securityLevel;
        _elevator = elevator;
        CurrentFloor = FloorLevel.G;
    }

    public void CallElevator(FloorLevel targetFloor)
    {
        Console.WriteLine($"{SecurityLevel} agent calling elevator from {CurrentFloor} to {targetFloor}");
        _elevator.RequestElevator(this, targetFloor);
    }
}

public class Elevator
{
    private FloorLevel _currentFloor;
    private bool _isMoving;
    private readonly object _lock = new object();

    public Elevator()
    {
        _currentFloor = FloorLevel.G;
        _isMoving = false;
    }

    public void RequestElevator(Agent agent, FloorLevel targetFloor)
    {
        lock (_lock)
        {
            while (_isMoving)
            {
                Monitor.Wait(_lock);
            }

            Console.WriteLine($"Elevator moving from {_currentFloor} to {targetFloor}");
            _isMoving = true;

            
            MoveToFloor(targetFloor);
            CheckSecurity(agent, targetFloor);
        }
    }

    private void MoveToFloor(FloorLevel targetFloor)
    {
        while (_currentFloor != targetFloor)
        {
            if (_currentFloor < targetFloor)
            {
                _currentFloor++;
            }
            else
            {
                _currentFloor--;
            }

            Console.WriteLine($"Elevator at {_currentFloor}");
            Thread.Sleep(1000); 
        }

        _isMoving = false;
        Monitor.PulseAll(_lock);
    }

    private void CheckSecurity(Agent agent, FloorLevel targetFloor)
    {
        if (CanAccess(agent.SecurityLevel, targetFloor))
        {
            Console.WriteLine($"{agent.SecurityLevel} agent allowed to exit at {targetFloor}");
        }
        else
        {
            Console.WriteLine($"{agent.SecurityLevel} agent denied access to {targetFloor}");
        }
    }

    private bool CanAccess(SecurityLevel securityLevel, FloorLevel floorLevel)
    {
        switch (floorLevel)
        {
            case FloorLevel.G:
                return true;
            case FloorLevel.S:
                return securityLevel >= SecurityLevel.Secret;
            case FloorLevel.T1:
            case FloorLevel.T2:
                return securityLevel == SecurityLevel.TopSecret;
            default:
                return false;
        }
    }
}


class Program
{
    static void Main(string[] args)
    {
        Elevator elevator = new Elevator();

        Agent[] agents = new Agent[]
        {
            new Agent(SecurityLevel.Confidential, elevator),
            new Agent(SecurityLevel.Secret, elevator),
            new Agent(SecurityLevel.TopSecret, elevator)
        };

        Thread[] agentThreads = new Thread[agents.Length];

        for (int i = 0; i < agents.Length; i++)
        {
            int agentIndex = i;
            agentThreads[agentIndex] = new Thread(() =>
            {
                Random random = new Random();
                while (true)
                {
                    FloorLevel targetFloor = (FloorLevel)random.Next(0, 4);
                    agents[agentIndex].CallElevator(targetFloor);
                    Thread.Sleep(random.Next(2000, 5000));
                }
            });

            agentThreads[agentIndex].Start();
        }

        Console.ReadLine();
    }
}