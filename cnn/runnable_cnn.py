import sys
from keras.models import model_from_json
from functions import array_functions
from functions import label_functions
import numpy


filename = "..\\dataset\\symbols.txt"
input_width = 50
input_height = 50

try:
    # For console run
    script_name = str(sys.argv[0])
    path_steps = script_name.split("\\")
    script_name = path_steps[len(path_steps) - 1]
    script_directory_path = str(sys.argv[0]).replace(script_name, "")
    array_path = str(sys.argv[1])

    # Debug
    # array_path = "..\\temp\\Test_0.txt"
    # script_directory_path = ""

    input = array_functions.read_array_from_file(array_path)
    input = array_functions.input_array_scaling(input, (input_width, input_height))
    input = input.reshape(1, input.shape[0], input.shape[1], 1)

    # Another debug
    # data = numpy.load("..\\dataset\\dataset.npz")
    # input = data["X_test"][0]
    # array_functions.array_to_image(input).show()
    # input = input.reshape(1, input.shape[0], input.shape[1], 1)

    json_file = open(script_directory_path + "cnn_data\\cnn5.json", "r")
    loaded_model_json = json_file.read()
    json_file.close()
    model = model_from_json(loaded_model_json)
    model.load_weights(script_directory_path + "cnn_data\\cnn5.h5")

    model.compile(loss="categorical_crossentropy",
            optimizer="adam",
            metrics=["accuracy"])

    result = model.predict_on_batch(input)

    label = label_functions.label_creator(result, script_directory_path + filename)
except:
    label = "Error"
finally:
    print(label, end="")
