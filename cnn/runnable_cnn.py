import sys
from keras.models import model_from_json
from functions import array_functions
from functions import label_functions
import numpy


filename = "..\\dataset\\symbols-with-point.json"
input_width = 32
input_height = 32

try:
    try:
        # For console run
        script_name = str(sys.argv[0])
        path_steps = script_name.split("\\")
        script_name = path_steps[len(path_steps) - 1]
        script_directory_path = str(sys.argv[0]).replace(script_name, "")
        array_path = str(sys.argv[1])
    except:
        # Debug
        array_path = "..\\temp\\2.txt"
        script_directory_path = ""

    input = array_functions.read_array_from_file(array_path)
    array_functions.array_to_image(input).show()
    input = array_functions.input_array_scaling(input, (input_width, input_height), "Black")
    array_functions.array_to_image(input).show()
    input = input.reshape(1, input.shape[0], input.shape[1], 1)


    # Debug
    # data = numpy.load("..\\dataset\\dataset-32x32.npz")
    # num = 140
    # input = data["X_test"][num]
    # print(data["Y_test"][num])
    # array_functions.array_to_image(input).show()
    # input = input.reshape(1, input.shape[0], input.shape[1], 1)


    json_file = open(script_directory_path + "cnn_data\\cnn13.json", "r")
    loaded_model_json = json_file.read()
    json_file.close()
    model = model_from_json(loaded_model_json)
    model.load_weights(script_directory_path + "cnn_data\\cnn13.h5")

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


    result = model.predict_on_batch(input)


    # Debug
    # print(result)
    # print(result.max())


    label = label_functions.label_creator(result, script_directory_path + filename)
except:
    label = "Error"
finally:
    print(label, end="")
