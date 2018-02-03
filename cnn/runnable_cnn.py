import sys
from keras.models import model_from_json
from functions import array_functions
from functions import label_functions
from PIL import Image


input_width = 28
input_height = 28

try:
    # For console run
    # script_name = str(sys.argv[0])
    # path_steps = script_name.split("\\")
    # script_name = path_steps[len(path_steps) - 1]
    # script_directory_path = str(sys.argv[0]).replace(script_name, "")
    # array_path = str(sys.argv[1])


    # Debug
    array_path = "test_data\\6_long.txt"
    script_directory_path = ""


    input = array_functions.read_array_from_file(array_path)
    input = array_functions.resize_array(input, input_width, input_height)

    json_file = open(script_directory_path + "cnn_data\\cnn1.json", "r")
    loaded_model_json = json_file.read()
    json_file.close()
    model = model_from_json(loaded_model_json)
    model.load_weights(script_directory_path + "cnn_data\\cnn1.h5")

    model.compile(loss="categorical_crossentropy",
            optimizer="adam",
            metrics=["accuracy"])

    X = array_functions.prepare_for_cnn(input)
    result = model.predict_on_batch(X)


    # Debug
    print(result)


    label = label_functions.label_creator(result)
    print(label)
except:
    print("Error")
