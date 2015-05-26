SHELL=/bin/bash
PROJ_NAME = $(shell basename `pwd`)
CONFIG_DIR = configs
VERSION = $(shell git describe --tags)
BRANCH := $(shell git rev-parse --abbrev-ref HEAD 2>&1)

ifdef TRAVIS_TAG
ZIP_CORE := RealScience-$(TRAVIS_TAG).zip
else
ZIP_CORE := RealScience-$(TRAVIS_BRANCH)_$(TRAVIS_BUILD_NUMBER).zip
endif

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

zip: meta
	zip -r $(ZIP_CORE) GameData

clean:
	-rm *.zip
	-rm GameData/RealScience/*.version
	-rm *.version

ifdef TRAVIS_TAG
deploy:
else
ifeq ($(TRAVIS_SECURE_ENV_VARS),true)
deploy:
	@curl --ftp-create-dirs -T ${ZIP_CORE} -u ${FTP_USER}:${FTP_PASSWD} ftp://stantonspacebarn.com/webapps/buildtracker/builds/RealScience/build_$(TRAVIS_BRANCH)_$(TRAVIS_BUILD_NUMBER)/$(ZIP_CORE)
	python buildServer.py all --project-id 1 --project-name RealScience --build-name $(TRAVIS_BRANCH)_$(TRAVIS_BUILD_NUMBER) --changelog changes.md --files $(ZIP_CORE)
else
deploy:
	echo No secure environment available. Skipping deploy.
endif
endif
