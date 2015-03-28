SHELL=/bin/bash
PROJ_NAME = $(shell basename `pwd`)
CONFIG_DIR = configs
VERSION = $(shell git describe --tags)
BRANCH := $(shell git rev-parse --abbrev-ref HEAD 2>&1)

ZIPFILE := $(PROJ_NAME)-$(TRAVIS_TAG).zip

all: clean meta zip
	cp -r GameData/RealScience/ ~/Dropbox/KSP/RealScience/

release: zip

ifdef TRAVIS_TAG
meta:
	python makeMeta.py $(TRAVIS_TAG)
	cp RealScience.version GameData/RealScience/RealScience.version
else
meta:
endif

zip: configs meta
	zip -r $(ZIPFILE) GameData

clean:
	-rm *.zip
	-rm GameData/RealScience/*.version
	-rm *.version
