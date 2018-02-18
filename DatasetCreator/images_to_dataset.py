import sys
import numpy
import json
from PIL import Image
from PIL import ImageDraw
from keras.utils import np_utils
sys.path.append("..\\cnn")
from functions import array_functions


def get_final_array(image):
    array = numpy.array(image.getdata())
    array = array.reshape(image.width, image.height, 1)
    array = 1 - array / 255
    return array

try:
    filename = sys.argv[1]
    images_directory = sys.argv[2]
    fonts_number = int(sys.argv[3])
    input_width = int(sys.argv[4])
    input_height = int(sys.argv[5])
    output_filename = sys.argv[6]
except:
    filename = "..\dataset\symbols.json"
    images_directory = "..\dataset\images"
    fonts_number = 5
    input_width = 32
    input_height = 32
    output_filename = "..\dataset\dataset-32x32.npz"

X_train = []
Y_train = []
X_test = []
Y_test = []

f = open(filename, 'r')
symbols_json = json.load(f)
f.close()
symbols = symbols_json["Symbols"]
symbols = symbols.split(" ")

symbols_heights = []
count = 0
for symbol in symbols:
    heights_sum = 0
    for image_count in range(0, fonts_number):
        # Loading and initial transformation
        image = Image.open(images_directory + "\\" + str(count) + "-" + str(image_count) + ".png")
        image = image.convert('L')
        image = array_functions.image_diagonal_mapping(image)
        image = array_functions.cut_symbol(image)

        heights_sum += image.height

        image = array_functions.input_image_scaling(image, (input_width, input_height), "White")

        # Normal
        array = get_final_array(image)
        X_train.append(array)
        Y_train.append(count)

        # Rotated
        array = get_final_array(array_functions.rotate(image, 3))
        X_train.append(array)
        Y_train.append(count)
        array = get_final_array(array_functions.rotate(image, -3))
        X_train.append(array)
        Y_train.append(count)
        array = get_final_array(array_functions.rotate(image, 5))
        X_train.append(array)
        Y_train.append(count)
        array = get_final_array(array_functions.rotate(image, -5))
        X_train.append(array)
        Y_train.append(count)

        # Noise
        array = get_final_array(array_functions.random_dots(image, 20))
        X_train.append(array)
        Y_train.append(count)
        array = get_final_array(array_functions.random_dots(image, 50))
        X_train.append(array)
        Y_train.append(count)

        # Scaling
        array = get_final_array(array_functions.scaling(image, (0.8, 0.8)))
        X_train.append(array)
        Y_train.append(count)
        array = get_final_array(array_functions.scaling(image, (1.2, 1.2)))
        X_train.append(array)
        Y_train.append(count)

        # Deformation
        array = get_final_array(array_functions.deformation(image, (1, 0.8), "White"))
        X_train.append(array)
        Y_train.append(count)
        array = get_final_array(array_functions.deformation(image, (0.8, 1), "White"))
        X_train.append(array)
        Y_train.append(count)

        # Noise for test
        array = get_final_array(array_functions.random_dots(image, 50))
        X_test.append(array)
        Y_test.append(count)

    symbol_height = {}
    symbol_height["Symbol"] = symbol
    symbol_height["Height"] = int(heights_sum / image_count)
    symbols_heights.append(symbol_height)

    print(str(count) + ": " + symbol)
    count += 1

X_train = numpy.array(X_train).astype('float32')
Y_train = np_utils.to_categorical(Y_train, count)
X_test = numpy.array(X_test).astype('float32')
Y_test = np_utils.to_categorical(Y_test, count)

symbols_json["SymbolsHeights"] = symbols_heights
f = open(filename, 'w')
json.dump(symbols_json, f)
f.close

print(X_train.shape)
print(Y_train.shape)
print(X_test.shape)
print(Y_test.shape)
print(symbols_json)

numpy.savez(output_filename, X_train=X_train, Y_train=Y_train, X_test=X_test, Y_test=Y_test)
