import React, { Component } from 'react';
import { connect } from 'react-redux'
import { Map, GoogleApiWrapper, InfoWindow, Marker } from 'google-maps-react';
import { fetchDevices } from '../../actions'

import { google_key } from '../../keys'

const mapStyles = {
  width: '65%',
  height: '50%'
};

export class MapContainer extends Component {

    constructor(props){
        super(props);
        this.state = {
            showingInfoWindow: false,  //Hides or the shows the infoWindow
            activeMarker: {},          //Shows the active marker upon click
            selectedPlace: {}          //Shows the infoWindow to the selected place upon a marker
        };
    }

    componentDidMount(){
        this.props.fetchDevices()
    }
    

    onMarkerClick = (props, marker, e) =>
        this.setState({
            selectedPlace: props,
            activeMarker: marker,
            showingInfoWindow: true
    });
    
    onClose = props => {
        if (this.state.showingInfoWindow) {
        this.setState({
            showingInfoWindow: false,
            activeMarker: null
        });
        }
    };

    displayMarkers(){
        return this.props.devices.map(device => {
            console.log(device)
            return <Marker key= {device.device_id} position={
                {
                    lat: device.lat,
                    lng: device.lng
                }
            }
            onClick={this.onMarkerClick}
            name = {device.device_id} />
        })
    }

  render() {
    if(!this.props.isSignedIn){
        return <div>Please signin</div>
    }
    if(!this.props.devices){
        return <div>Loading...</div>
    }
    return (
      <Map
        google={this.props.google}
        zoom={14}
        style={mapStyles}
        initialCenter={{
         lat: 32.115221,
         lng: 34.798248
        }}
      >
      {this.displayMarkers()}
      <InfoWindow
          marker={this.state.activeMarker}
          visible={this.state.showingInfoWindow}
          onClose={this.onClose}
        >
          <div>
            <h4>{this.state.selectedPlace.name}</h4>
          </div>
        </InfoWindow>
      </Map>
    );
  }
}

const mapStateToProps = (state) => {
    //Object.values gets all the object inside and make it as array
    return {
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn,
        devices: Object.values(state.devices)
    }
}

export default connect(mapStateToProps,{fetchDevices})(GoogleApiWrapper({
  apiKey: google_key
})(MapContainer));