using DayZEngine, DataFrames, StatsBase

function dayz_economy_simulator()
    println("DayZ Economy Simulator")
    println("=" * 70)
    
    world = World()
    
    economy = Dict(
        "items" => DataFrame(
            name = ["Bandage", "Water", "Food", "Ammo", "Weapon", "Backpack"],
            spawn_rate = [0.8, 0.6, 0.7, 0.3, 0.1, 0.4],
            value = [1, 2, 3, 5, 10, 8],
            weight = [0.1, 0.5, 0.3, 0.2, 3.0, 2.0],
            rarity = [3, 2, 2, 4, 5, 4]
        ),
        "spawn_points" => 150,
        "players" => 50,
        "map_size" => Vec3(15360, 500, 15360),
        "loot_decay_rate" => 0.001f0
    )
    
    println("Map Configuration:")
    println("  Size: $(economy["map_size"])")
    println("  Spawn points: $(economy["spawn_points"])")
    println("  Initial players: $(economy["players"])")
    println("  Items: $(nrow(economy["items"]))")
    
    println("\nItem Catalogue:")
    for row in eachrow(economy["items"])
        println("  • $(row.name): value=$(row.value), rarity=$(row.rarity), weight=$(row.weight)")
    end
    
    loot_spawned = zeros(Int, nrow(economy["items"]))
    loot_consumed = zeros(Int, nrow(economy["items"]))
    loot_despawned = zeros(Int, nrow(economy["items"]))
    
    sim_time = 0.0f0
    dt = 60.0f0
    max_time = 3600.0f0
    
    println("\n" * 70)
    println("Simulation Running...")
    println("=" * 70)
    
    tick = 0
    while sim_time < max_time
        tick += 1
        sim_time += dt
        
        for (idx, row) in enumerate(eachrow(economy["items"]))
            items_in_world = loot_spawned[idx] - loot_consumed[idx] - loot_despawned[idx]
            
            spawn_chance = row.spawn_rate * (1.0f0 - items_in_world / 1000.0f0)
            
            if rand() < spawn_chance
                loot_spawned[idx] += Int(round(economy["spawn_points"] / nrow(economy["items"])))
            end
            
            if rand() < 0.1f0 * economy["loot_decay_rate"]
                loot_despawned[idx] += 1
            end
            
            if rand() < 0.05f0
                loot_consumed[idx] += 1
            end
        end
        
        if tick % 10 == 0
            total_items = sum(loot_spawned .- loot_consumed .- loot_despawned)
            println("\nTick $(tick) ($(Int(sim_time))s):")
            println("  Total items in world: $total_items")
            
            println("  Item distribution:")
            for (idx, item_name) in enumerate(economy["items"].name)
                count = loot_spawned[idx] - loot_consumed[idx] - loot_despawned[idx]
                if count > 0
                    pct = Int(round(100 * count / max(total_items, 1)))
                    println("    $(item_name): $count ($pct%)")
                end
            end
            
            economy_value = sum(
                (loot_spawned[idx] - loot_consumed[idx] - loot_despawned[idx]) * 
                economy["items"][idx, :value]
                for idx in 1:nrow(economy["items"])
            )
            println("  Total economy value: $economy_value")
        end
    end
    
    println("\n" * 70)
    println("Simulation Complete!")
    println("=" * 70)
    
    println("\nFinal Statistics:")
    total_spawned = sum(loot_spawned)
    total_consumed = sum(loot_consumed)
    total_despawned = sum(loot_despawned)
    
    println("  Total items spawned: $total_spawned")
    println("  Total items consumed: $total_consumed")
    println("  Total items despawned: $total_despawned")
    println("  Items still in world: $(total_spawned - total_consumed - total_despawned)")
    
    println("\nPer-item breakdown:")
    results = DataFrame(
        item = economy["items"].name,
        spawned = loot_spawned,
        consumed = loot_consumed,
        despawned = loot_despawned,
        remaining = loot_spawned .- loot_consumed .- loot_despawned,
        value = economy["items"].value
    )
    
    for row in eachrow(results)
        total_value = row.remaining * row.value
        println("  $(row.item):")
        println("    Remaining: $(row.remaining), Value: $total_value")
    end
    
    return economy, results
end

function analyze_loot_distribution()
    println("\nLoot Distribution Analysis")
    println("=" * 70)
    
    spawn_map = zeros(Int, 10, 10)
    
    for _ in 1:1000
        x = rand(1:10)
        z = rand(1:10)
        spawn_map[x, z] += 1
    end
    
    println("\nSpawn heatmap (10x10 grid, 1000 samples):")
    for z in 1:10
        row = ""
        for x in 1:10
            count = spawn_map[x, z]
            char = count < 5 ? "." : count < 20 ? "o" : count < 50 ? "O" : "#"
            row *= char
        end
        println("  $row")
    end
    
    println("\nDensity statistics:")
    println("  Mean spawns per cell: $(mean(spawn_map))")
    println("  Max spawns in cell: $(maximum(spawn_map))")
    println("  Min spawns in cell: $(minimum(spawn_map))")
    println("  Std deviation: $(std(spawn_map))")
end

economy, results = dayz_economy_simulator()
analyze_loot_distribution()
