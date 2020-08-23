import React, { Component } from 'react';
import { connect } from 'react-redux'
import { fetchDevices } from '../../actions'

import { Map, TileLayer, Marker, Popup, ZoomControl } from 'react-leaflet';

const position = { lng: 34.798248, lat: 32.115221 };

export class MapContainer2 extends Component {

    componentDidMount(){
        this.props.fetchDevices()
    }

    displayMarkers(){
        return this.props.devices.map(device => {
            return (
                <Marker key= {device.device_id} position={[device.lan, device.lng]}>
                    <Popup>
                        {device.device_id}
                    </Popup>
                </Marker>
            )
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
            <div>
                <h3>Devices on the map</h3>
                <Map style={{ width: '100%', height: '400px'}} center={position} zoom={13}>
                <ZoomControl position="topright" />
                    <TileLayer
                        url='http://{s}.tile.osm.org/{z}/{x}/{y}.png'
                        attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
                    />
                    {this.displayMarkers()}
                </Map>
            </div>

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

export default connect(mapStateToProps,{fetchDevices})(MapContainer2);