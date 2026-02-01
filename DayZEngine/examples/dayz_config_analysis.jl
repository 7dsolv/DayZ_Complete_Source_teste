using DayZEngine, DataFrames

function analyze_dayz_configs()
    println("DayZ Config Analysis Tool")
    println("=" * 50)
    
    config_files = ARGS[1:end]
    
    for config_file in config_files
        if !isfile(config_file)
            println("File not found: $config_file")
            continue
        end
        
        println("\nAnalyzing: $config_file")
        analyze_config(config_file)
    end
end

function analyze_config(file_path::String)
    _, ext = splitext(file_path)
    
    if ext == ".xml"
        analyze_xml_config(file_path)
    elseif ext == ".json"
        analyze_json_config(file_path)
    elseif ext == ".cpp"
        analyze_cpp_config(file_path)
    else
        println("Unsupported file type: $ext")
    end
end

function analyze_xml_config(file_path::String)
    try
        data = parse_xml_config(file_path)
        
        println("XML Structure:")
        print_dict(data, 0, 5)
        
        if occursin("economy", lowercase(file_path))
            items_df = parse_economy_config(file_path)
            println("\nParsed $(nrow(items_df)) items")
            if nrow(items_df) > 0
                println(first(items_df, 5))
            end
        elseif occursin("spawn", lowercase(file_path))
            spawns_df = parse_spawn_points(file_path)
            println("\nParsed $(nrow(spawns_df)) spawn points")
            if nrow(spawns_df) > 0
                println(first(spawns_df, 5))
            end
        end
    catch e
        println("Error parsing XML: $e")
    end
end

function analyze_json_config(file_path::String)
    try
        data = parse_json_config(file_path)
        println("JSON Structure:")
        print_dict(data, 0, 5)
    catch e
        println("Error parsing JSON: $e")
    end
end

function analyze_cpp_config(file_path::String)
    try
        classes = parse_config_cpp(file_path)
        println("Classes found: $(length(classes))")
        for (class_name, class_data) in classes
            println("\nClass: $class_name")
            println("  Properties: $(length(class_data))")
            for (key, value) in take(class_data, 5)
                println("    $key = $value")
            end
            if length(class_data) > 5
                println("    ... and $(length(class_data) - 5) more properties")
            end
        end
    catch e
        println("Error parsing CPP: $e")
    end
end

function print_dict(data::Dict, depth::Int=0, max_depth::Int=3)
    if depth > max_depth
        return
    end
    
    indent = "  " ^ depth
    for (key, value) in take(data, 5)
        if isa(value, Dict)
            println("$indent$key: <Dict>")
            print_dict(value, depth + 1, max_depth)
        elseif isa(value, Vector)
            println("$indent$key: <Vector($(length(value)))>")
        else
            println("$indent$key: $value")
        end
    end
    
    if length(data) > 5
        println("$indent... and $(length(data) - 5) more entries")
    end
end

if length(ARGS) == 0
    println("Usage: julia dayz_config_analysis.jl <config_file1> [<config_file2> ...]")
    println("Supported formats: .xml, .json, .cpp")
else
    analyze_dayz_configs()
end
