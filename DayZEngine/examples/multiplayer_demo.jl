using DayZEngine

function multiplayer_example()
    mode = ARGS[1]
    
    if mode == "server"
        server_mode()
    elseif mode == "client"
        client_mode()
    else
        println("Usage: julia multiplayer_example.jl [server|client]")
    end
end

function server_mode()
    engine = Engine(30)
    initialize!(engine)
    
    world = get_world(engine)
    network = NetworkManager(0)
    start_server!(network, 5555)
    engine.network_manager = network
    
    player1 = create_entity!(world, "Player1")
    add_component!(world, player1, Transform(Vec3(0, 0, 0)))
    add_component!(world, player1, RigidBody(1.0f0))
    
    function server_update(w::World, dt::Float32)
        replicate_entity_state!(network, player1, 
                               get_component(w, player1, Transform).position,
                               get_component(w, player1, Transform).rotation,
                               get_component(w, player1, RigidBody).velocity)
    end
    
    add_system!(engine, server_update)
    
    println("Server started on port 5555")
    println("Running server for 100 frames...")
    
    for i in 1:100
        tick!(engine, 1.0f0 / 30.0f0)
        if i % 30 == 0
            println("Server: Frame $i, Players connected: $(get_connected_peer_count(network))")
        end
    end
    
    shutdown!(engine)
end

function client_mode()
    engine = Engine(30)
    initialize!(engine)
    
    world = get_world(engine)
    network = NetworkManager(1)
    
    if connect_to_server!(network, "localhost", 5555)
        println("Connected to server")
    else
        println("Failed to connect to server")
        return
    end
    
    player = create_entity!(world, "Player")
    add_component!(world, player, Transform(Vec3(5, 0, 5)))
    add_component!(world, player, RigidBody(1.0f0))
    
    function client_update(w::World, dt::Float32)
        broadcast_message!(network, "player_update", Dict(
            "position" => [1.0, 0.0, 0.0],
            "velocity" => [0.5, 0.0, 0.0]
        ))
    end
    
    add_system!(engine, client_update)
    
    println("Client running...")
    
    for i in 1:100
        tick!(engine, 1.0f0 / 30.0f0)
        if i % 30 == 0
            println("Client: Frame $i")
        end
    end
    
    shutdown!(engine)
end

multiplayer_example()
