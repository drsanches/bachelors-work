import sys
from keras.models import model_from_json
from functions import array_functions
from functions import label_functions
import numpy


filename = "..\\dataset\\symbols.json"
input_width = 32
input_height = 32
model_filepath = "cnn_data\\cnn15.json"
weights_filepath = "cnn_data\\cnn15.h5"


try:
    try:
        # For console run
        script_name = str(sys.argv[0])
        path_steps = script_name.split("\\")
        script_name = path_steps[len(path_steps) - 1]
        script_directory_path = str(sys.argv[0]).replace(script_name, "")
        if (len(sys.argv) < 2):
            raise "Input arguments error";
        array_paths = []
        for i in range(1, len(sys.argv)):
            array_paths.append(str(sys.argv[i]))
    except:
        # Debug
        array_paths = ["..\\temp\\1-1.txt", "..\\temp\\1-2.txt", "..\\temp\\1-3.txt"]
        script_directory_path = ""


    inputs = []
    for array_path in array_paths:
        input = array_functions.read_array_from_file(array_path)
        # array_functions.array_to_image(input).show()
        input = array_functions.input_array_scaling(input, (input_width, input_height), "Black")
        # array_functions.array_to_image(input).show()
        input = input.reshape(1, input.shape[0], input.shape[1], 1)
        inputs.append(input)


    # Debug
    # data = numpy.load("..\\dataset\\dataset-32x32.npz")
    # num = 41
    # input = data["X_test"][num]
    # print(data["Y_test"][num])
    # array_functions.array_to_image(input).show()
    # input = input.reshape(1, input.shape[0], input.shape[1], 1)
    # inputs = [input]


    json_file = open(script_directory_path + model_filepath, "r")
    loaded_model_json = json_file.read()
    json_file.close()
    model = model_from_json(loaded_model_json)
    model.load_weights(script_directory_path + weights_filepath)

    model.compile(loss="categorical_crossentropy",
            optimizer="adam",
            metrics=["accuracy"])


    # Debug
    # data = numpy.load("..\\dataset\\dataset-32x32.npz")
    # X_test = data["X_test"]
    # Y_test = data["Y_test"]
    # for i in range(0, len(data["X_test"])):
    #     x1 = X_test[i].reshape(1, X_test.shape[1], X_test.shape[2], 1)
    #     y1 = Y_test[i].reshape(1, Y_test.shape[1])
    #     scores = model.evaluate(x1, y1, verbose=0)
    #     print(str(i) + " - %.2f%%" % (scores[1]*100))


    label = ""
    for input in inputs:
        result = model.predict_on_batch(input)

        # Debug
        # print(result)
        # print(result.max())

        label += label_functions.label_creator(result, script_directory_path + filename) + " "
except:
    label = "Runtime error!"
finally:
    print(label, end="")
