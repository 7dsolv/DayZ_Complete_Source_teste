using DayZEngine, Printf

println("DayZEngine - Quick Start Guide")
println("=" * 70)
println()

println("STEP 1: Import and Create Engine")
println("-" * 70)

code1 = """
using DayZEngine

engine = Engine(60)                # 60 FPS
initialize!(engine)
world = get_world(engine)
"""

println(code1)

println("\nSTEP 2: Create Entities")
println("-" * 70)

code2 = """
# Create a player entity
player = create_entity!(world, "Player")
add_component!(world, player, Transform(Vec3(0, 0, 0)))
add_component!(world, player, RigidBody(1.0f0))
add_component!(world, player, SphereCollider(0.5f0))

# Create ground
ground = create_entity!(world, "Ground")
add_component!(world, ground, Transform(Vec3(0, -5, 0)))
rb = RigidBody(0.0f0)
rb.is_static = true
add_component!(world, ground, rb)
add_component!(world, ground, BoxCollider(Vec3(50, 1, 50)))
"""

println(code2)

println("\nSTEP 3: Add Physics")
println("-" * 70)

code3 = """
physics = PhysicsEngine()
engine.physics_engine = physics

function physics_system(w::World, dt::Float32)
    update!(physics, w, dt)
end

add_system!(engine, physics_system)
"""

println(code3)

println("\nSTEP 4: Add Custom Behavior (Systems)")
println("-" * 70)

code4 = """
function movement_system(w::World, dt::Float32)
    player_id = get_entity_by_name(w, "Player")
    if player_id !== nothing
        rb = get_component(w, player_id, RigidBody)
        transform = get_component(w, player_id, Transform)
        
        if rb !== nothing && transform !== nothing
            # Apply forces, check input, etc
            add_force!(rb, Vec3(0, 0, 1))
        end
    end
end

add_system!(engine, movement_system)
"""

println(code4)

println("\nSTEP 5: Run Game Loop")
println("-" * 70)

code5 = """
for frame in 1:1000
    tick!(engine, 1.0f0 / 60.0f0)
    
    if frame % 60 == 0
        player_id = get_entity_by_name(world, "Player")
        pos = get_component(world, player_id, Transform).position
        @printf("Frame %d - Player at (%.2f, %.2f, %.2f)\\n", 
                frame, pos[1], pos[2], pos[3])
    end
end

shutdown!(engine)
"""

println(code5)

println("\n" * 70)
println("COMMON OPERATIONS")
println("=" * 70)

operations = Dict(
    "Query entities with component" => 
        "entities = get_entities_with(world, RigidBody)",
    
    "Apply force to entity" =>
        "add_force!(rb, Vec3(10, 0, 0))",
    
    "Set entity position" =>
        "set_position!(transform, Vec3(5, 0, 5))",
    
    "Rotate entity" =>
        "rotate!(transform, Vec3(0, 1, 0), π/4)",
    
    "Tag entity" =>
        "set_entity_tag!(world, entity_id, \"enemy\")",
    
    "Get entities by tag" =>
        "enemies = get_entities_by_tag(world, \"enemy\")",
    
    "Save world" =>
        "save_world_to_json(world, \"world.json\")",
    
    "Send network message" =>
        "broadcast_message!(network, \"attack\", Dict(\"damage\" => 10))",
    
    "Attach script to entity" =>
        "attach_script!(script_mgr, entity_id, script)",
    
    "Parse DayZ config" =>
        "items = parse_economy_config(\"cfgeconomycore.xml\")"
)

for (op, code) in pairs(operations)
    println("\n$op:")
    println("  $code")
end

println("\n" * 70)
println("EXAMPLE: Complete Minimal Game")
println("=" * 70)

minimal_game = """
using DayZEngine

function minimal_game()
    # Setup
    engine = Engine(60)
    initialize!(engine)
    world = get_world(engine)
    
    # Create player
    player = create_entity!(world, "Player")
    add_component!(world, player, Transform(Vec3(0, 5, 0)))
    add_component!(world, player, RigidBody(1.0f0))
    
    # Add physics
    physics = PhysicsEngine()
    engine.physics_engine = physics
    add_system!(engine, (w, dt) -> update!(physics, w, dt))
    
    # Run
    for i in 1:300
        tick!(engine, 1.0f0 / 60.0f0)
    end
    
    shutdown!(engine)
end

minimal_game()
"""

println(minimal_game)

println("\n" * 70)
println("DEBUGGING TIPS")
println("=" * 70)

tips = [
    "Check entity existence: get_entity(world, entity_id) !== nothing",
    "List all entities: keys(world.entity_manager.entities)",
    "Check components: has_component(world, entity_id, ComponentType)",
    "Print position: println(get_component(world, e, Transform).position)",
    "Monitor FPS: println(get_fps(engine))",
    "Check frame count: println(get_frame(engine))",
    "List all entities with type: get_entities_with(world, ComponentType)"
]

for tip in tips
    println("  • $tip")
end

println("\n" * 70)
println("Next step: Check examples/ folder for complete working code!")
println("=" * 70)
