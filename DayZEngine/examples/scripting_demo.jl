using DayZEngine

function scripting_example()
    engine = Engine(60)
    initialize!(engine)
    
    world = get_world(engine)
    
    player = create_entity!(world, "Player")
    add_component!(world, player, Transform(Vec3(0, 0, 0)))
    add_component!(world, player, RigidBody(1.0f0))
    add_component!(world, player, SphereCollider(0.5f0))
    
    script_manager = ScriptManager()
    engine.script_manager = script_manager
    
    script_content = """
        mutable struct PlayerController
            speed::Float32
            jump_force::Float32
        end
        
        controller = PlayerController(5.0f0, 10.0f0)
        
        function on_update(world, dt)
            player_id = 1
            transform = get_component(world, player_id, Transform)
            rb = get_component(world, player_id, RigidBody)
            
            if transform !== nothing && rb !== nothing
                forward_input = 0.0f0
                right_input = 0.0f0
                
                move_dir = Vec3(right_input, 0, -forward_input)
                if norm(move_dir) > 0.01f0
                    move_dir = normalize(move_dir) .* controller.speed
                    rb.velocity = Vec3(move_dir[1], rb.velocity[2], move_dir[3])
                end
            end
        end
        
        function on_collision_enter(world, entity_a, entity_b)
            println("Collision detected between entities \$entity_a and \$entity_b")
        end
    """
    
    script_file = "/tmp/player_controller.jl"
    open(script_file, "w") do f
        write(f, script_content)
    end
    
    player_script = Script("PlayerController", script_file)
    attach_script!(script_manager, player, player_script)
    load_script(script_manager, "PlayerController", script_file)
    
    function physics_system(w::World, dt::Float32)
        physics = PhysicsEngine()
        update!(physics, w, dt)
    end
    
    add_system!(engine, physics_system)
    
    function script_system(w::World, dt::Float32)
        update_scripts!(script_manager, w, dt)
    end
    
    add_system!(engine, script_system)
    
    println("Scripting example running...")
    for i in 1:180
        tick!(engine, 1.0f0 / 60.0f0)
        
        if i % 60 == 0
            player_transform = get_component(world, player, Transform)
            println("Frame $i - Player at $(player_transform.position)")
        end
    end
    
    shutdown!(engine)
    rm(script_file)
    println("Scripting example complete")
end

scripting_example()
