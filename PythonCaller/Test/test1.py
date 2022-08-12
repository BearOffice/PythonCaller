import csconnector as csc

class Weather(object):
    def __init__(self, temp, description):
        self.temp = temp
        self.description = description

input = csc.get_input()
input = Weather(**input)

input.temp += 12.8
input.description = "Too hot"

csc.set_output(input)
