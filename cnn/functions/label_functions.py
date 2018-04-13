import json

def label_creator(cnn_out, filename):
    cnn_out = cnn_out[0]
    index = cnn_out.argmax()

    if float(cnn_out[index]) < 0.30:
        return "Error"
    else:
        f = open(filename, 'r')
        symbols = json.load(f)
        f.close()
        symbols = symbols["Symbols"]
        symbols = symbols.split(" ")
        return symbols[index]
