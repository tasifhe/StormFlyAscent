# Bird Character Controller

A state machine-based bird character controller for Unity using the CharacterController component.

## Features

- **State Machine Architecture**: Clean separation of movement states
- **Multiple Flight States**: Flying, Gliding, Diving
- **Ground Movement**: Walking and jumping
- **Smooth Transitions**: Automatic state changes based on conditions
- **Animation Integration**: Animator parameter control
- **Physics-Based**: Realistic gravity, momentum, and air resistance

## Setup

1. Attach the `Character` script to your bird GameObject
2. Add a `CharacterController` component
3. Add an `Animator` component with bird animations
4. Configure the parameters in the Inspector

## Controls

### Ground Movement
- **WASD/Arrow Keys**: Move around
- **Space**: Jump (transitions to flying)

### Flight Controls
- **Space**: Flap wings (gain height)
- **WASD/Arrow Keys**: Steer during flight
- **S/Down Arrow**: Dive
- **Release Space**: Enter glide mode

## States

### StandingState
- Ground-based movement
- Smooth rotation towards movement direction
- Transitions to FlyingState on jump

### JumpingState
- Initial jump from ground
- Air control with reduced movement
- Transitions to FlyingState

### FlyingState
- Active wing flapping
- Strong upward thrust when flapping
- Steering control
- Transitions to GlidingState or DivingState

### GlidingState
- Passive flight with momentum
- Reduced gravity effect
- Steering with limited vertical control
- Transitions back to FlyingState on flap

### DivingState
- Fast downward movement
- Increased speed and momentum
- Recovery possible with flap input
- Transitions to GlidingState or ground

## Animation Parameters

Set up these parameters in your Animator Controller:

- `IsFlying` (bool): Active when flapping
- `IsGliding` (bool): Active during glide
- `IsDiving` (bool): Active during dive
- `IsGrounded` (bool): Active when on ground
- `IsFlapping` (bool): True during flap animation
- `Speed` (float): Movement speed on ground
- `VerticalSpeed` (float): Current vertical velocity

## Parameters

### Ground Movement
- `playerSpeed`: Ground movement speed
- `rotationSpeed`: How fast character turns
- `jumpHeight`: Initial jump force

### Flight
- `flapStrength`: Upward force from wing flaps
- `glideSpeed`: Forward speed during glide
- `diveSpeed`: Maximum dive speed
- `turnSpeed`: Steering responsiveness
- `liftForce`: Passive upward force
- `airResistance`: Air friction (0-1)

### Animation
- `speedDampTime`: Smoothing for speed changes
- `airControl`: Movement control in air (0-1)

## Usage Tips

1. **Tune Physics**: Adjust flap strength and air resistance for desired flight feel
2. **Animation States**: Create corresponding animation states in your Animator
3. **Camera**: Consider adding a follow camera that adjusts for flight
4. **Audio**: Add wing flap sounds and wind effects
5. **Particles**: Wing trail effects during fast flight

## Extending

The state machine is easily extensible:
- Add new states by inheriting from `State`
- Modify transition conditions in `ChangeState()`
- Add new input handling in `HandleInput()`
- Customize physics in `PhysicsUpdate()`

## Troubleshooting

- **Not Flying**: Check CharacterController height and center settings
- **Jittery Movement**: Adjust air resistance and physics timestep
- **No Transitions**: Verify input detection and state conditions
- **Animation Issues**: Ensure Animator parameters match exactly