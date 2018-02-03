import sys
from keras.models import model_from_json
from functions import array_functions
from functions import label_functions


try:
    script_name = str(sys.argv[0])
    path_steps = script_name.split("\\")
    script_name = path_steps[len(path_steps) - 1]
    script_directory_path = str(sys.argv[0]).replace(script_name, "")
    array_path = str(sys.argv[1])

    symbol = array_functions.read_array_from_file(array_path)

    json_file = open(script_directory_path + "cnn_data\\cnn1.json", "r")
    loaded_model_json = json_file.read()
    json_file.close()
    model = model_from_json(loaded_model_json)
    model.load_weights(script_directory_path + "cnn_data\\cnn1.h5")

    model.compile(loss="categorical_crossentropy",
            optimizer="adam",
            metrics=["accuracy"])

    X = symbol.reshape(1, symbol.shape[0], symbol.shape[1], 1)
    result = model.predict_on_batch(X)

    label = label_functions.label_creator(result)
    print(label)
except:
    print("Error")
